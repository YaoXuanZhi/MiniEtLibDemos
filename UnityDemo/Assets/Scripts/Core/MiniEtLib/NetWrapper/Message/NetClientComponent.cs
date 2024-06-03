using System.Net.Sockets;
using System.Collections.Generic;

namespace ET.Client
{
    public struct NetClientComponentOnRead
    {
        public Session Session;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetClientComponent: Entity, IAwake<AddressFamily>, IAwake<string>, IDestroy
    {
        public int ServiceId;
    }
}