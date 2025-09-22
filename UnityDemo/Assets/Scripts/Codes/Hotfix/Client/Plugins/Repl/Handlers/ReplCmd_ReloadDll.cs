using System;

namespace ET.Client
{
    [ReplCommand("logintest", "测试登录")]
    public class ReplCmd_LoginTest : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            await TestHelper.LoginTest(repl.DomainScene());
        }
    }
}