using ET.Server;

namespace ET
{
    [ReplCommand("gm", "执行GM命令（用法: gm <命令> [参数...]）")]
    public class ReplCmd_Gm : IReplCommandHandler
    {
        public async ETTask Run(ReplComponent repl, string content, string[] args)
        {
            if (args.Length == 0)
            {
                var comp = ReplComponentSystem.GetGmDispatcher();
                if (comp == null) { ReplComponentSystem.PrintError("GmDispatcherComponent not found."); return; }

                Console.WriteLine($"{"GM Name",-25} {"Handlers",-10}");
                Console.WriteLine(new string('─', 40));
                foreach (var kv in comp.Handlers.OrderBy(k => k.Key))
                    Console.WriteLine($"{kv.Key,-25} {kv.Value.Count,-10}");
                return;
            }

            var gmName = args[0];
            var gmArgs = args.Length > 1 ? args[1..].ToList() : new List<string>();

            var dispatcher = ReplComponentSystem.GetGmDispatcher();
            if (dispatcher == null) { ReplComponentSystem.PrintError("GmDispatcherComponent not found."); return; }

            try
            {
                var result = dispatcher.ProxyGmCommand(null, null, gmName, gmArgs);
                if (string.IsNullOrEmpty(result))
                    ReplComponentSystem.PrintSuccess($"GM '{gmName}' executed.");
                else
                    ReplComponentSystem.PrintSuccess($"GM '{gmName}' result: {result}");
            }
            catch (Exception ex)
            {
                ReplComponentSystem.PrintException(ex);
            }
            await ETTask.CompletedTask;
        }
    }
}

