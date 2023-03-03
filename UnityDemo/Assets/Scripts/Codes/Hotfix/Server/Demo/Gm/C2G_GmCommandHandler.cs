using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class C2G_GmCommandHandler : AMRpcHandler<C2G_GmCommand, G2C_GmCommand>
	{
		protected override async ETTask Run(Session session, C2G_GmCommand request, G2C_GmCommand response)
		{
			var player = session.GetComponent<SessionPlayerComponent>()?.Player;
			if (player == null)
			{
				response.Error = ErrorCore.ERR_NotFoundActor;
				return;
			}

			response.Message = Root.Instance.Scene.GetComponent<GmDispatcherComponent>().ProxyGmCommand(session, player, request.Command, request.CommandArgs);
			await ETTask.CompletedTask;
		}
	}
}