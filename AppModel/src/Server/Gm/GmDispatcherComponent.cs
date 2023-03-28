using System.Collections.Generic;

namespace ET.Server
{
    public class GmDispatcherInfo
    {
        public string GmName { get; }
        public IGmHandler IGmHandler { get; }

        public GmDispatcherInfo(string gmName, IGmHandler imHandler)
        {
            this.GmName = gmName;
            this.IGmHandler = imHandler;
        }
    }
    
    /// <summary>
    /// Gm分发组件，参考MessageDispatcherComponent实现
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class GmDispatcherComponent: Entity, IAwake, IDestroy, ILoad
    {
        //gmName => gmHandlers
        public readonly Dictionary<string, List<GmDispatcherInfo>> Handlers = new Dictionary<string, List<GmDispatcherInfo>>();
    }

}