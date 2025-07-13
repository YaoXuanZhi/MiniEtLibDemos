using System;
using ET;
using CommandLine;

namespace TestCase
{
    public static class Bootstrap
    {
        public static void Start()
        {
            try
            {	
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Log.Error(e.ExceptionObject.ToString());
                };
				
                // 异步方法全部会回掉到主线程
                Game.AddSingleton<MainThreadSynchronizationContext>();


                // 构造测试用例的服务器启动参数
                {
                    Game.AddSingleton<Options>();
                    Options.Instance.StartConfig = "StartConfig/Localhost";
                    Options.Instance.Process = 1;
                }

                Game.AddSingleton<TimeInfo>();

                Game.AddSingleton<Logger>().ILog = new NUnitLogger();
                Game.AddSingleton<ObjectPool>();
                Game.AddSingleton<IdGenerater>();
                Game.AddSingleton<EventSystem>();
                Game.AddSingleton<TimerComponent>();
                Game.AddSingleton<CoroutineLockComponent>();
				
                ETTask.ExceptionHandler += Log.Error;
				
                Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");

                Game.AddSingleton<CodeLoader>().Start();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void Update()
        {
            Game.Update();
        }

        public static void LateUpdate()
        {
            Game.LateUpdate();
        }

        public static void FrameFinishUpdate()
        {
            Game.FrameFinishUpdate();
        }
    }
}