using System;
using System.Collections.Generic;
using System.IO;
using Bright.Serialization;

namespace ET.Server
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigComponent.GetAllConfigBytes, Dictionary<Type, ByteBuf>>
    {
        public override Dictionary<Type, ByteBuf> Handle(ConfigComponent.GetAllConfigBytes args)
        {
            Dictionary<Type, ByteBuf> output = new Dictionary<Type, ByteBuf>();
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };
            HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute));
            foreach (Type configType in configTypes)
            {
                string configFilePath;
                if (startConfigs.Contains(configType.Name))
                {
                    configFilePath = $"../../Resources/Config/Excel/s/{Options.Instance.StartConfig}/{configType.Name.ToLower()}.bytes";    
                }
                else
                {
                    configFilePath = $"../../Resources/Config/Excel/cs/GameConfig/{configType.Name.ToLower()}.bytes";
                }
                output[configType] = new ByteBuf(File.ReadAllBytes(configFilePath));
            }

            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigComponent.GetOneConfigBytes, ByteBuf>
    {
        public override ByteBuf Handle(ConfigComponent.GetOneConfigBytes args)
        {
            var configFilePath = $"../../Resources/Config/Excel/cs/GameConfig/{args.ConfigName.ToLower()}.bytes";
            ByteBuf configBytes = new ByteBuf(File.ReadAllBytes(configFilePath));
            return configBytes;
        }
    }
}