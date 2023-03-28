using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    [GmHandler("error_test", "错误码测试")]
    public class GmErrorTest : IGmHandler
    {
        public string Handle(Session session, Player player, List<string> gmArgs)
        {
            session.Send(new G2C_Message() { Message = "error_test" });
            return string.Empty;
        }
    }
}