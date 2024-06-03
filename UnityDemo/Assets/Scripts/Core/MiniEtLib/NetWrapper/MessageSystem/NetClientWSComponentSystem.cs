using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(NetClientWSComponent))]
    public static class NetClientWSComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientWSComponent, IEnumerable<string>>
        {
            protected override void Awake(NetClientWSComponent self, IEnumerable<string> prefixs)
            {
#if UNITY_WEBGL
                self.ServiceId = NetServices.Instance.AddService(new UnityWebsocketService());
#else
                self.ServiceId = NetServices.Instance.AddService(new WService());
#endif
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientWSComponent>
        {
            protected override void Destroy(NetClientWSComponent self)
            {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
        }

        private static void OnRead(this NetClientWSComponent self, long channelId, long actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetClientComponentOnRead() {Session = session, Message = message});
        }

        private static void OnError(this NetClientWSComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        public static Session Create(this NetClientWSComponent self, IPEndPoint realIPEndPoint)
        {
            long channelId = NetServices.Instance.CreateConnectChannelId();
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, realIPEndPoint);

            return session;
        }
        
        public static Session Create(this NetClientWSComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
        {
            long channelId = localConn;
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark)
            {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, routerIPEndPoint);

            return session;
        }
    }
}