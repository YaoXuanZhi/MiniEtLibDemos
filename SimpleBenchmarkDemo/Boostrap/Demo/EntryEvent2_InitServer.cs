using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args)
        {
            var zone = 1;
            var sceneType = SceneType.BenchmarkServer;
            var name = "BenchmarkServer";

            Scene appScene = EntitySceneFactory.CreateScene(zone, sceneType, name);
            appScene.AddComponent<BenchmarkServerComponent>();
            appScene.AddComponent<NetServerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint("127.0.0.1", 30001));

            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.GetOrAddComponent<ConsoleComponent>();
            }
        }
    }
}