using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitClient: AEvent<ET.EventType.EntryEvent3>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent3 args)
        {
            var zone = 1;
            var sceneType = SceneType.BenchmarkClient;
            var name = "BenchmarkClient";

            Scene appScene = EntitySceneFactory.CreateScene(zone, sceneType, name);
            appScene.AddComponent<BenchmarkClientComponent>();

            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.GetOrAddComponent<ConsoleComponent>();
            }
            await ETTask.CompletedTask;
        }
    }
}