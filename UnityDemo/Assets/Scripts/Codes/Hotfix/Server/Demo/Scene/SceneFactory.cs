using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ET.Server
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateServerScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null)
        {
            await ETTask.CompletedTask;
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);

            switch (scene.SceneType)
            {
                case SceneType.Gate:
                    // scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                    scene.AddComponent<NetServerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint("127.0.0.1", 30001));
                    scene.AddComponent<NetServerWSComponent, IEnumerable<string>>(new[]{$"http://127.0.0.1:30301/"});
                    scene.AddComponent<PlayerComponent>();
                    break;
                case SceneType.BenchmarkServer:
                    scene.AddComponent<BenchmarkServerComponent>();
                    scene.AddComponent<NetServerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint("127.0.0.1", 30001));
                    // scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.OuterIPPort);
                    break;
                case SceneType.BenchmarkClient:
                    scene.AddComponent<BenchmarkClientComponent>();
                    break;
            }

            return scene;
        }
    }
}