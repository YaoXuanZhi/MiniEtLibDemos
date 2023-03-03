using System;
using System.Collections.Generic;

namespace ET.Server
{
    public static class EventSystemExtensions
    {
        public static void Publish<E, T>(this EventSystem eventSystem, E entity, T a) where E: Entity where T : struct
        {
            eventSystem.PublishShare(entity, a);
        }
        
        public static async ETTask PublishAsync<E, T>(this EventSystem eventSystem, E entity, T a) where E: Entity where T : struct
        {
            await eventSystem.PublishAsyncShare(entity, a);
        }
    }
}