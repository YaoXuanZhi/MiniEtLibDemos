using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [FriendOf(typeof(NetClientComponent))]
    public static class NetClientComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientComponent, AddressFamily>
        {
            protected override void Awake(NetClientComponent self, AddressFamily addressFamily)
            {
                self.ServiceId = NetServices.Instance.AddService(new TService(addressFamily, ServiceType.Outer));
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientComponent>
        {
            protected override void Destroy(NetClientComponent self)
            {
#if UNITY_EDITOR
                //在Unity编辑器里，停止运行的时候，原本逻辑无法正常释放端口
                NetServices.Instance.DirectRemoveService(self.ServiceId);
#else
                NetServices.Instance.RemoveService(self.ServiceId);
#endif
            }
        }

        private static void OnRead(this NetClientComponent self, long channelId, long actorId, object message)
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

        private static void OnError(this NetClientComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        public static Session Create(this NetClientComponent self, IPEndPoint realIPEndPoint)
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
        
        public static Session Create(this NetClientComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
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