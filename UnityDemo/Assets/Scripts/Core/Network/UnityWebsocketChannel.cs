#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityWebSocket;

namespace ET
{
    public class UnityWebsocketChannel: AChannel
    {
        private readonly UnityWebsocketService Service;
        private IWebSocket webSocket;
        private IWebSocket wsTemp;
        
        private bool isConnected;
        private bool isSending;
        
        private readonly Queue<MemoryStream> queue = new Queue<MemoryStream>();
        
        private readonly MemoryStream recvStream;

        public UnityWebsocketChannel(long id, IPEndPoint ipEndPoint, UnityWebsocketService service)
        {
            this.Service = service;
            this.Id = id;
            this.recvStream = new MemoryStream(ushort.MaxValue);
            isConnected = false;
            
            wsTemp = new WebSocket($"ws://{ipEndPoint}");

            this.RemoteAddress = ipEndPoint;

            // Subscribe to the WS events
            wsTemp.OnOpen += OnOpen;
            wsTemp.OnClose += OnClosed;
            wsTemp.OnError += OnError;
            wsTemp.OnMessage += OnRead;

            // Start connecting to the server
            wsTemp.ConnectAsync();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            this.webSocket?.CloseAsync();
            this.webSocket = null;
            
            long id = this.Id;
            this.Id = 0;
            this.Service.Remove(id);
        }

        public void Send(MemoryStream memoryStream)
        {
            switch (this.Service.ServiceType)
            {
                case ServiceType.Inner:
                    break;
                case ServiceType.Outer:
                    memoryStream.Seek(Packet.ActorIdLength, SeekOrigin.Begin);
                    break;
            }

            if (this.webSocket == null)
            {
                this.queue.Enqueue(memoryStream);
                return;
            }

            SendOne(memoryStream);
        }
        
        private void SendOne(MemoryStream memoryStream)
        {
            var buffer = new ReadOnlyMemory<byte>(memoryStream.GetBuffer(), (int)memoryStream.Position, (int)(memoryStream.Length - memoryStream.Position));
            this.webSocket.SendAsync(buffer.ToArray(), 0, buffer.Length);
        }

        private void OnOpen(object sender, OpenEventArgs e)
        {
            /*if (ws == null)
            {
                this.OnError(ErrorCore.ERR_WebsocketConnectError);
                return;
            }*/

            if (this.IsDisposed)
            {
                return;
            }

            isConnected = true;
            this.webSocket = wsTemp;
                
            while (this.queue.Count > 0)
            {
                MemoryStream memoryStream = this.queue.Dequeue();
                this.SendOne(memoryStream);
            }
        }
        
        /// <summary>
        /// Called when we received a text message from the server
        /// </summary>
        private void OnRead(object sender, MessageEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }

            var receiveCount = e.RawData.Length;
            this.recvStream.SetLength(receiveCount);
            this.recvStream.Seek(2, SeekOrigin.Begin);
            Array.Copy(e.RawData, 0, this.recvStream.GetBuffer(), 0, receiveCount);
            this.OnRead(this.recvStream);
        }
        
        private void OnRead(MemoryStream memoryStream)
        {
            try
            {
                long channelId = this.Id;
                object message = null;
                long actorId = 0;
                switch (this.Service.ServiceType)
                {
                    case ServiceType.Outer:
                    {
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode);
                        message = SerializeHelper.Deserialize(type, memoryStream);
                        break;
                    }
                    case ServiceType.Inner:
                    {
                        actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode);
                        message = SerializeHelper.Deserialize(type, memoryStream);
                        break;
                    }
                }
                NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message);
            }
            catch (Exception e)
            {
                Log.Error($"{this.RemoteAddress} {memoryStream.Length} {e}");
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                this.OnError(ErrorCore.ERR_PacketParserError);
            }
        }
        
        /// <summary>
        /// Called when the web socket closed
        /// </summary>
        private void OnClosed(object sender, CloseEventArgs e)
        {
            isConnected = false;
            
            if (this.IsDisposed)
            {
                return;
            }
            
            Log.Error($"wchannel closed: StatusCode: {e.StatusCode}, Reason: {e.Reason}");
            this.OnError(0);
        }
        
        /// <summary>
        /// Called when an error occured on client side
        /// </summary>
        private void OnError(object sender, UnityWebSocket.ErrorEventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            Log.Error($"WChannel error: {this.Id} {e.Message}");
            
            this.OnError(ErrorCore.ERR_WebsocketError);
        }
        
        private void OnError(int error)
        {
            Log.Info($"UnityWebsocketChannel error: {error} {this.RemoteAddress}");
			
            long channelId = this.Id;
			
            this.Service.Remove(channelId);
			
            NetServices.Instance.OnError(this.Service.Id, channelId, error);
        }
    }
}
#endif