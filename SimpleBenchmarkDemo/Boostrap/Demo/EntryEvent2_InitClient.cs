using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitClient: AEvent<ET.EventType.EntryEvent3>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent3 args)
        {
            Root.Instance.Scene.GetOrAddComponent<ServerSceneManagerComponent>();

            var id = 2;
            var instanceId = IdGenerater.Instance.GenerateInstanceId();
            var zone = 1;
            var sceneType = SceneType.BenchmarkClient;
            var name = "BenchmarkClient";

            await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, id, instanceId, zone, name,
                sceneType);

            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.GetOrAddComponent<ConsoleComponent>();
            }
            await ETTask.CompletedTask;
        }
    }
}