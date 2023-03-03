using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class TestHelper
    {
        public static async ETTask LoginTest(Scene clientScene)
        {
            var account = "test";
            await LoginHelper.Login(clientScene, account, "");
            await LoginHelper.Logout(clientScene);
        }
    }
}