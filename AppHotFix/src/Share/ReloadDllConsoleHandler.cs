namespace ET
{
    [ConsoleHandler(ConsoleMode.ReloadDll)]
    public class ReloadDllConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContext context, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadDll:
                    context.Parent.RemoveComponent<ModeContext>();
                    
                    CodeLoader.Instance.LoadHotfix();
                    
                    EventSystem.Instance.Load();
                    Console.WriteLine("DLL熱重載已完成");
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}