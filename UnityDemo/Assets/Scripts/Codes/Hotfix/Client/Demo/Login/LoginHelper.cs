using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask<string> Login(Scene clientScene, string account, string password)
        {
            Log.Debug("开始连接。。。");
            NetClientComponent netClientComponent = null;
            Session session = null;

            if (Define.IsUnityStandaloneWebGL)
            {
                netClientComponent = clientScene.GetOrAddComponent<NetClientComponent, string>("ws:");
                session = netClientComponent.Create(NetworkHelper.ToIPEndPoint("127.0.0.1", 30301));
            }
            else
            {
                netClientComponent = clientScene.AddComponent<NetClientComponent, AddressFamily>(AddressFamily.InterNetwork);
                session = netClientComponent.Create(NetworkHelper.ToIPEndPoint("127.0.0.1", 30001));
            }
            
            clientScene.GetOrAddComponent<SessionComponent>().Session = session;
        
            Log.Debug("登录服务器。。。");
            var response = (G2C_CreateRole) await session.Call(new C2G_CreateRole() { Name = account });
            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"CreateRole失败, ErrorCode:{response.Error} Message:{response.Message}");
                return "create_role_fail";
            }
            Log.Debug("登录完成");

            await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());            
            
            await PingTest(clientScene);

            return string.Empty;
        }

        private static async ETTask PingTest(Scene scene)
        {
            NetClientComponent netClientComponent = scene.GetComponent<NetClientComponent>();
            foreach (var child in netClientComponent.Children.Values)
            {
                if (child is Session session)
                {
                    Log.Debug("开始Ping");
                    var response = (G2C_Ping)await session.Call(new C2G_Ping() { });
                    Log.Debug($"结束Ping {response.Time}");
                }
            }
        }

        public static async ETTask<string> Logout(Scene scene)
        {
            Log.Debug("开始登出。。。");
            
            var session = scene.GetComponent<SessionComponent>().Session;
            var response = (G2C_RoleLogout)await session.Call(new C2G_RoleLogout() {});
            scene.RemoveComponent<NetClientComponent>();

            Log.Debug("玩家登出完成");
            return response.Message;
        }
    }
}