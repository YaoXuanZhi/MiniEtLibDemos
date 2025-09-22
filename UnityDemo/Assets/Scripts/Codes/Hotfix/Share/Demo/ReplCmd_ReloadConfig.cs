using System;

namespace ET
{
    [ReplCommand("reloadconfig", "Reload a config table (usage: reloadconfig <Name>)")]
    public class ReplCmd_ReloadConfig : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: reloadconfig <ConfigName>");
                Console.WriteLine("Example: reloadconfig UnitConfig");
                Console.WriteLine("The config class must exist as ET.<Name>Category");
                return;
            }

            var configName = args[0];
            var category = $"{configName}Category";
            var type = EventSystem.Instance.GetType($"ET.{category}");
            if (type == null)
            {
                ReplComponentSystem.PrintError($"Config type not found: ET.{category}");
                return;
            }

            try
            {
                ConfigComponent.Instance.LoadOneConfig(type);
                ReplComponentSystem.PrintSuccess($"Config '{configName}' reloaded.");
            }
            catch (Exception ex)
            {
                ReplComponentSystem.PrintException(ex);
            }
            await ETTask.CompletedTask;
        }
    }
}