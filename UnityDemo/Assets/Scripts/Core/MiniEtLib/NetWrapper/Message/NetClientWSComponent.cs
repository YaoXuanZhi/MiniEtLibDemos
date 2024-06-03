using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class NetClientWSComponent: Entity, IAwake<IEnumerable<string>>, IDestroy
    {
        public int ServiceId;
    }
}