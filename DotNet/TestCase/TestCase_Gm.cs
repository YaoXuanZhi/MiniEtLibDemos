using ET;

namespace TestCase;

[TestFixture(TestName = "测试用例-Gm命令")]
public class TestCase_Gm
{
    [TestCase(TestName = "测试用例-执行Gm指令成功")]
    public async Task TestCaseExecuteGmSuccess()
    {
        var clientScene = await ServerGlobalSetup.CreateClientScene();
        
        var account = "testcase";
        var password = "testcase";
        await ServerGlobalSetup.Login(clientScene, account, password);
        
        var errorMessage = await ServerGlobalSetup.GmCall(clientScene, $"error_test");
        Assert.IsTrue(string.IsNullOrEmpty(errorMessage), "error_test失败");
        
        errorMessage = await ServerGlobalSetup.GmCall(clientScene, $"do_example");
        Assert.That(errorMessage, Is.EqualTo("do_example_ok"), "do_example失败");
         
        await ServerGlobalSetup.Logout(clientScene);
    }
    
    [TestCase(TestName = "测试用例-执行Gm指令失败")]
    public async Task TestCaseExecuteGmFail()
    {
        var clientScene = await ServerGlobalSetup.CreateClientScene();
        
        var account = "testcase";
        var password = "testcase";
        await ServerGlobalSetup.Login(clientScene, account, password);
        
        var errorMessage = await ServerGlobalSetup.GmCall(clientScene, $"add_exp 10000");
        Assert.That(errorMessage, Is.EqualTo("not_found_gm_config"), "Gm命令还没支持");
         
        await ServerGlobalSetup.Logout(clientScene);
    }
}
