using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class NetServerWSComponent: Entity, IAwake<IEnumerable<string>>, IAwake, IDestroy
    {
        public int ServiceId;
    }
}