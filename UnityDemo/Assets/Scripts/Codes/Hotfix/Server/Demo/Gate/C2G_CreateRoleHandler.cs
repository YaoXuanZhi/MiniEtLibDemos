using System;
using System.Collections.Generic;


namespace ET.Server
{
    [MessageHandler(SceneType.Gate)]
    public class C2G_CreateRoleHandler : AMRpcHandler<C2G_CreateRole, G2C_CreateRole>
    {
        protected override async ETTask Run(Session session, C2G_CreateRole request, G2C_CreateRole response)
        {
            var player = session.GetComponent<SessionPlayerComponent>()?.Player;
            if (player != null)
            {
                response.Error = ErrorCode.ERR_GATE_IS_ONLINE;
                return;
            }

            Scene scene = session.DomainScene();
            PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
            playerComponent.LoggingCount++;
			
            player = playerComponent.AddChild<Player, string>(request.Name);
            
            playerComponent.Add(player);
            session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
            Log.Debug($"player.Id Login ===> {playerComponent.LoggingCount} {player.Id}");

            var instanceId = session.InstanceId;
            using var @lock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GateLogin, player.UserId);
            using var offlineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GateLogout, player.UserId);
    
            if (instanceId != session.InstanceId)
            {
                response.Error = ErrorCore.ERR_NotFoundActor;
                return;
            }
        }
    }
}