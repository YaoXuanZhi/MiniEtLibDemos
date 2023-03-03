namespace ET.Client
{
    [ConsoleHandler(ConsoleMode.LoginTest)]
    public class LoginTestConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContext context, string content)
        {
            switch (content)
            {
                case ConsoleMode.LoginTest:
                    await TestHelper.LoginTest(context.DomainScene());
                    context.Parent.RemoveComponent<ModeContext>();
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}