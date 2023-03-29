using System.Net;

namespace ET.Client
{
    [Event(SceneType.Process)]
    public class EntryEvent2_InitClient: AEvent<ET.EventType.EntryEvent3>
    {
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent3 args)
        {
            await TestHelper.LoginTest(scene);

            if (Options.Instance.Console == 1)
            {
                Root.Instance.Scene.GetOrAddComponent<ConsoleComponent>();
            }
        }
    }
}