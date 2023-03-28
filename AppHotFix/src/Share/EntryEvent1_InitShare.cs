using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ET
{
    [Event(SceneType.Process)]
    public class EntryEvent1_InitShare: AEvent<EventType.EntryEvent1>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args)
        {
            Root.Instance.Scene.AddComponent<NetThreadComponent>();
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>();
            
            await ETTask.CompletedTask;
        }
    }
}