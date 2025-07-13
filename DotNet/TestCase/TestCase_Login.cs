using ET;

namespace TestCase;

[TestFixture(TestName = "玩家登录&登出")]
public class TestCase_Login
{
    [TestCase("testcase1", "testcase1", TestName = "测试用例-玩家1")]
    [TestCase("testcase2", "testcase2", TestName = "测试用例-玩家2")]
    [TestCase("testcase3", "testcase3", TestName = "测试用例-玩家3")]
    public async Task TestCaseLogin(string account, string password)
    {
        var clientScene = await ServerGlobalSetup.CreateClientScene();
        
        var errorMessage = await ServerGlobalSetup.Login(clientScene, account, password);
        Assert.IsTrue(string.IsNullOrEmpty(errorMessage), $"{account} 登录失败");
        
        errorMessage = await ServerGlobalSetup.Logout(clientScene);
        Assert.IsTrue(string.IsNullOrEmpty(errorMessage), $"{account} 登出失败");
        
        Log.Debug($"玩家{account} 登录&登出成功");
    }
}
