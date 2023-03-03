using System;
using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// Gm指令分发组件
    /// </summary>
    [FriendOf(typeof(GmDispatcherComponent))]
    public static class GmDispatcherComponentSystem
    {
        [ObjectSystem]
        public class GmDispatcherComponentAwakeSystem: AwakeSystem<GmDispatcherComponent>
        {
            protected override void Awake(GmDispatcherComponent self)
            {
                self.Load();
            }
        }

        [ObjectSystem]
        public class GmDispatcherComponentLoadSystem: LoadSystem<GmDispatcherComponent>
        {
            protected override void Load(GmDispatcherComponent self)
            {
                self.Load();
            }
        }

        [ObjectSystem]
        public class GmDispatcherComponentDestroySystem: DestroySystem<GmDispatcherComponent>
        {
            protected override void Destroy(GmDispatcherComponent self)
            {
                self.Handlers.Clear();
            }
        }

        private static void Load(this GmDispatcherComponent self)
        {
            self.Handlers.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (GmHandlerAttribute));

            foreach (Type type in types)
            {
                IGmHandler gmHandler = Activator.CreateInstance(type) as IGmHandler;
                if (gmHandler == null)
                {
                    Log.Error($"gm handle {type.Name} 需要继承 IGmHandler");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(GmHandlerAttribute), false);
                
                foreach (object attr in attrs)
                {
                    if (attr is GmHandlerAttribute gmHandlerAttribute)
                    {
                        GmDispatcherInfo messageDispatcherInfo = new (gmHandlerAttribute.GmName, gmHandler);
                        self.RegisterHandler(gmHandlerAttribute.GmName, messageDispatcherInfo);
                    }
                }
            }
        }

        private static void RegisterHandler(this GmDispatcherComponent self, string gmName, GmDispatcherInfo handler)
        {
            if (!self.Handlers.ContainsKey(gmName))
            {
                self.Handlers.Add(gmName, new List<GmDispatcherInfo>());
            }

            self.Handlers[gmName].Add(handler);
        }

        public static string ProxyGmCommand(this GmDispatcherComponent self, Session session, Player player, string gmName, List<string> gmArgs)
        {
            if (!self.Handlers.TryGetValue(gmName, out var actions))
            {
                Log.Error($"该Gm没有处理，没有找到使用[GmHandler(\"{gmName}\")]的地方");
                return "not_found_gm_config";
            }

            string errId = string.Empty;
            foreach (GmDispatcherInfo ev in actions)
            {
                try
                {
                    errId = ev.IGmHandler.Handle(session, player, gmArgs);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return errId;
        }
    }
}