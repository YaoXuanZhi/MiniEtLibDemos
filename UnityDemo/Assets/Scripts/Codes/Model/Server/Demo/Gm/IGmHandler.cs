using System;
using System.Collections.Generic;

namespace ET.Server
{
    public interface IGmHandler
    {
        string Handle(ET.Session session, Server.Player player, List<string> gmArgs);
    }
}