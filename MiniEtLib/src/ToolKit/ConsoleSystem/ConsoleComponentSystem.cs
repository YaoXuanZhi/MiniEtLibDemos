using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    [FriendOf(typeof(ConsoleComponent))]
    [FriendOf(typeof(ModeContext))]
    public static class ConsoleComponentSystem
    {
        [ObjectSystem]
        public class ConsoleComponentAwakeSystem: AwakeSystem<ConsoleComponent>
        {
            protected override void Awake(ConsoleComponent self)
            {
                self.Load();
            
                self.Start().Coroutine();
            }
        }

        [ObjectSystem]
        public class ConsoleComponentLoadSystem: LoadSystem<ConsoleComponent>
        {
            protected override void Load(ConsoleComponent self)
            {
                self.Load();
            }
        }
        
        public static void Load(this ConsoleComponent self)
        {
            self.Handlers.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (ConsoleHandlerAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ConsoleHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                ConsoleHandlerAttribute consoleHandlerAttribute = (ConsoleHandlerAttribute)attrs[0];

                object obj = Activator.CreateInstance(type);

                IConsoleHandler iConsoleHandler = obj as IConsoleHandler;
                if (iConsoleHandler == null)
                {
                    throw new Exception($"ConsoleHandler handler not inherit IConsoleHandler class: {obj.GetType().FullName}");
                }
                self.Handlers.Add(consoleHandlerAttribute.Mode, iConsoleHandler);
            }
        }
        
        public static async ETTask Start(this ConsoleComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    ModeContext modeContext = self.GetComponent<ModeContext>();
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{modeContext?.Mode ?? ""}> ");
                        return Console.In.ReadLine();
                    }, self.CancellationTokenSource.Token);
                    
                    line = line.Trim();

                    switch (line)
                    {
                        case "":
                            break;
                        case "exit":
                            self.RemoveComponent<ModeContext>();
                            break;
                        default:
                        {
                            string[] lines = line.Split(" ");
                            string mode = modeContext == null? lines[0] : modeContext.Mode;

                            if (!self.Handlers.TryGetValue(mode, out IConsoleHandler iConsoleHandler))
                            {
                                Log.Console($"not found command: {line}");
                                break;
                            }

                            if (modeContext == null)
                            {
                                modeContext = self.AddComponent<ModeContext>();
                                modeContext.Mode = mode;
                            }
                            await iConsoleHandler.Run(modeContext, line);
                            break;
                        }
                    }


                }
                catch (Exception e)
                {
                    Log.Console(e.ToString());
                }
            }
        }
    }
}