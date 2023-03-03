using System;

namespace ET
{
    [ConsoleHandler(ConsoleMode.ReloadConfig)]
    public class ReloadConfigConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContext context, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadConfig:
                    context.Parent.RemoveComponent<ModeContext>();
                    Log.Console("C must have config name, like: C UnitConfig");
                    foreach (var row in AIConfigCategory.Instance.DataList)
                    {
                        Log.Debug($"{row.Name} => {row.Desc}");
                    }
                    break;
                default:
                    string[] ss = content.Split(" ");
                    string configName = ss[1];
                    string category = $"{configName}Category";
                    Type type = EventSystem.Instance.GetType($"ET.{category}");
                    if (type == null)
                    {
                        Log.Console($"reload config but not find {category}");
                        return;
                    }
                    ConfigComponent.Instance.LoadOneConfig(type);
                    Log.Console($"reload config {configName} finish!");

                    context.Parent.RemoveComponent<ModeContext>();
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}