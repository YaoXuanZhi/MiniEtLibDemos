using System;
using System.Collections.Generic;


namespace ET.Server
{
    [MessageHandler(SceneType.Gate)]
    public class C2G_RoleLogoutHandler : AMRpcHandler<C2G_RoleLogout, G2C_RoleLogout>
    {
        protected override async ETTask Run(Session session, C2G_RoleLogout request, G2C_RoleLogout response)
        {
            var player = session.GetComponent<SessionPlayerComponent>()?.Player;
            if (player == null)
            {
                response.Error = ErrorCode.ERR_GATE_NOT_LOGIN;
                return;
            }

            Scene scene = session.DomainScene();
            PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
            playerComponent.LoggingCount--;
            Log.Info($"player.Id Logout ===> {playerComponent?.LoggingCount} {player.Id}");
            playerComponent?.Remove(player.Id);
            await ETTask.CompletedTask;
        }
    }
}