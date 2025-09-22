using System;

namespace ET
{
    [ReplCommand("reloaddll", "Hot-reload Hotfix DLL")]
    public class ReplCmd_ReloadDll : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            try
            {
                CodeLoader.Instance.LoadHotfix();
                EventSystem.Instance.Load();
                ReplComponentSystem.PrintSuccess("Hotfix DLL reloaded successfully.");
            }
            catch (Exception ex)
            {
                ReplComponentSystem.PrintException(ex);
            }
            await ETTask.CompletedTask;
        }
    }
}