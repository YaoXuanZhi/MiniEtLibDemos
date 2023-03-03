using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class G2C_MessageHandler : AMHandler<G2C_Message>
    {
        protected override async ETTask Run(Session session, G2C_Message message)
        {
            var clientScene = session.DomainScene();
            var uiComponent = clientScene.GetComponent<UIComponent>();
            UI ui = uiComponent.Get(UIType.UILobby);
            var uiLobbyComponent = ui.GetComponent<UILobbyComponent>();
            uiLobbyComponent.MessageText.text = $"响应了：{message.Message}";
            await ETTask.CompletedTask;
        }
    }
}
