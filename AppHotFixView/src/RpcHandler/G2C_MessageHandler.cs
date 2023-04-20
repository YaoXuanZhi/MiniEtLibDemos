using System.Collections.Generic;

namespace ET.Client
{
    [MessageHandler(SceneType.Process)]
    public class G2C_MessageHandler : AMHandler<G2C_Message>
    {
        protected override async ETTask Run(Session session, G2C_Message message)
        {
            var clientScene = session.DomainScene();
            var mainWindowComponent = clientScene.GetComponent<MainWindowComponent>();
            mainWindowComponent.textbox_message.Text = $"{message.Message}";
            await ETTask.CompletedTask;
        }
    }
}
