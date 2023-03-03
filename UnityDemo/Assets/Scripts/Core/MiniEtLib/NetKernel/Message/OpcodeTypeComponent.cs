using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class OpcodeTypeComponent: Entity, IAwake, IDestroy
    {
        [StaticField]
        public static OpcodeTypeComponent Instance;
        
        public HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        
        public readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
    }
}