using ET;
using ET.Client;

namespace TestCase
{
    [SetUpFixture]
    public static class ServerGlobalSetup
    {
        private static GameServerLoop GameLoop { get; set; }
        private static Thread _loopThread;

        [OneTimeSetUp]
        public static async Task RunBeforeAnyTests()
        {
            Entry.Init();
            Bootstrap.Start();

            GameLoop = new GameServerLoop();
            _loopThread = new Thread(GameLoop.Start);
            GameLoop.OnUpdate += OnGameLoop;
            _loopThread.IsBackground = true;
            _loopThread.Start();
            await ETTask.CompletedTask;
        }

        [OneTimeTearDown]
        public static async Task RunAfterAllTests()
        {
            ET.Game.Close();

            //需要确保ET的Component都回收
            await Task.Delay(5000);

            GameLoop.Stop();
            _loopThread.Join(1000); // 等待线程退出
        }

        private static void OnGameLoop()
        {
            try
            {
                Bootstrap.Update();
                Bootstrap.LateUpdate();
                Bootstrap.FrameFinishUpdate();
            }
            catch (Exception e)
            {
                ET.Log.Error(e);
            }
        }
        
        private static int zone = 100;
        public static async ETTask<Scene> CreateClientScene()
        {
            zone++;
            ET.Scene clientScene = await SceneFactory.CreateClientScene(zone, $"TestCase_{zone}");
            return clientScene;
        }

        public static async ETTask<string> Login(Scene clientScene, string accountName, string password)
        {
            return await LoginHelper.Login(clientScene, accountName, password);
        }

        public static async ETTask<string> Logout(Scene scene)
        {
            return await LoginHelper.Logout(scene);
        }
        
        public static async ETTask<string> GmCall(Scene clientScene, string gmCommand)
        {
            var args = gmCommand.Split(" ");
            var session = clientScene.GetComponent<SessionComponent>().Session;
            var request = new C2G_GmCommand();
                   
            request.CommandArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                {
                    request.Command = args[i];
                }
                else
                {
                    request.CommandArgs.Add(args[i]);
                }
            }
         
            var response = await session.Call(request);
            return response.Message;
        }
    }
}
