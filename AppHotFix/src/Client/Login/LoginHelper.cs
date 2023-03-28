using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene clientScene, string account, string password)
        {
            Console.WriteLine("开始连接。。。");
            NetClientComponent netClientComponent = clientScene.AddComponent<NetClientComponent, AddressFamily>(AddressFamily.InterNetwork);
            var session = netClientComponent.Create(NetworkHelper.ToIPEndPoint("127.0.0.1", 30001));
        
            Console.WriteLine("登录服务器。。。");
            var response = (G2C_CreateRole) await session.Call(new C2G_CreateRole() { Name = account });
            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"CreateRole失败, ErrorCode:{response.Error} Message:{response.Message}");
                return;
            }
            Console.WriteLine("登录完成");

            await PingTest(clientScene);
        }

        private static async ETTask PingTest(Scene scene)
        {
            NetClientComponent netClientComponent = scene.GetComponent<NetClientComponent>();
            foreach (var child in netClientComponent.Children.Values)
            {
                if (child is Session session)
                {
                    Console.WriteLine("开始Ping");
                    var response = (G2C_Ping)await session.Call(new C2G_Ping() { });
                    Console.WriteLine($"结束Ping {response.Time}");
                }
            }
        }

        public static async ETTask Logout(Scene scene)
        {
            NetClientComponent netClientComponent = scene.GetComponent<NetClientComponent>();
            foreach (var child in netClientComponent.Children.Values)
            {
                if (child is Session session)
                {
                    var response = (G2C_RoleLogout)await session.Call(new C2G_RoleLogout() { });
                    scene.RemoveComponent<NetClientComponent>();
                    
                    Console.WriteLine($"玩家登出");
                }
            }
        }
        
    }
}