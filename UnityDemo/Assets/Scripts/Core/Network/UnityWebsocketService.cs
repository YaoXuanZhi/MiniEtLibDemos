#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ET
{
    public class UnityWebsocketService: AService
    {
        private long idGenerater = 200000000;
        
        private readonly Dictionary<long, UnityWebsocketChannel> channels = new Dictionary<long, UnityWebsocketChannel>();
        
        public UnityWebsocketService()
        {
            this.ServiceType = ServiceType.Outer;
        }
        
        private long GetId
        {
            get
            {
                return ++this.idGenerater;
            }
        }

        public override void Remove(long id, int error = 0)
        {
            UnityWebsocketChannel channel;
            if (!this.channels.TryGetValue(id, out channel))
            {
                return;
            }

            channel.Error = error;

            this.channels.Remove(id);
            channel.Dispose();
        }

        public override bool IsDispose()
        {
            return this.Id == 0;
        }

        protected void Get(long id, IPEndPoint ipEndPoint)
        {
            if (!this.channels.TryGetValue(id, out _))
            {
                this.Create(id, ipEndPoint);
            }
        }
        
        public UnityWebsocketChannel Get(long id)
        {
            UnityWebsocketChannel channel = null;
            this.channels.TryGetValue(id, out channel);
            return channel;
        }

        public override void Dispose()
        {
            if (this.IsDispose())
            {
                return;
            }
            
            base.Dispose();

            this.Id = 0;

            foreach (var kv in this.channels.ToArray())
            {
                kv.Value.Dispose();
            }
        }
        
        private UnityWebsocketChannel Create(IPEndPoint ipEndPoint, long id)
        {
            UnityWebsocketChannel channel = new UnityWebsocketChannel(id, ipEndPoint, this);
            this.channels.Add(channel.Id, channel);
            return channel;
        }

        public override void Create(long id, IPEndPoint ipEndpoint)
        {
            if (this.channels.TryGetValue(id, out UnityWebsocketChannel _))
            {
                return;
            }
            this.Create(ipEndpoint, id);
        }

        public override void Send(long channelId, long actorId, object message)
        {
            this.channels.TryGetValue(channelId, out var channel);
            if (channel == null)
            {
                return;
            }
            MemoryStream memoryStream = this.GetMemoryStream(message);
            channel.Send(memoryStream);
        }

        public override void Update()
        {
        }
    }
}
#endif