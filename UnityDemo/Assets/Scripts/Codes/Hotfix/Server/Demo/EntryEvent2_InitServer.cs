using System.Net;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<ET.EventType.EntryEvent2>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args)
        {
            Root.Instance.Scene.AddComponent<GmDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ServerSceneManagerComponent>();

            {
                var id = 1;
                var instanceId = 1;
                var zone = 1;
                var sceneType = SceneType.Gate;
                var name = "GateServer";
                
                await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, id, instanceId, zone, name,
                    sceneType);
            }

            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.GetOrAddComponent<ConsoleComponent>();
            }
        }
    }
}