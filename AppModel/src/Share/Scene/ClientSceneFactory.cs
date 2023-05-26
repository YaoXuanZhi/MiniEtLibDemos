namespace ET.Client
{
    public static class SceneFactory
    {
        public static async ETTask<Scene> CreateClientScene(int zone, string name)
        {
            await ETTask.CompletedTask;
            
            Scene clientScene = EntitySceneFactory.CreateScene(zone, SceneType.Client, name, ClientSceneManagerComponent.Instance);
            clientScene.AddComponent<ObjectWait>();
            
            EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene());
            return clientScene;
        }
    }
}