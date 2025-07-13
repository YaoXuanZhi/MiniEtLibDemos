#if NETCOREAPP

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class G2C_MessageHandler : AMHandler<G2C_Message>
    {
        protected override async ETTask Run(Session session, G2C_Message message)
        {
            Log.Debug($"飘字：{message.Message}");
            await ETTask.CompletedTask;
        }
    }
}

#endif