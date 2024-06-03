using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    [FriendOf(typeof(NetServerWSComponent))]
    public static class NetServerWSComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem : AwakeSystem<NetServerWSComponent, IEnumerable<string>>
        {
            protected override void Awake(NetServerWSComponent self, IEnumerable<string> prefixs)
            {
                self.ServiceId = NetServices.Instance.AddService(new WService(prefixs));
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetServerWSComponent>
        {
            protected override void Destroy(NetServerWSComponent self)
            {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
        }

        private static void OnError(this NetServerWSComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetServerWSComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);

            if (self.DomainScene().SceneType != SceneType.BenchmarkServer)
            {
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                session.AddComponent<SessionAcceptTimeoutComponent>();
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
                session.AddComponent<SessionIdleCheckerComponent>();
            }
        }
        
        private static void OnRead(this NetServerWSComponent self, long channelId, long actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            session.LastRecvTime = TimeInfo.Instance.ClientNow();
            
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetServerComponentOnRead() {Session = session, Message = message});
        }
        
        // public static Session Create(this NetServerWSComponent self, IPEndPoint realIPEndPoint)
        // {
        //     long channelId = NetServices.Instance.CreateConnectChannelId();
        //     Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
        //     if (self.DomainScene().SceneType != SceneType.Benchmark)
        //     {
        //         session.AddComponent<SessionIdleCheckerComponent>();
        //     }
        //     NetServices.Instance.CreateChannel(self.ServiceId, session.Id, realIPEndPoint);
        //
        //     return session;
        // }
    }
}