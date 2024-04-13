using System.IO;
using UnityEngine;

namespace ET
{
    public static class ToolsEditor
    {
        public static void ExcelExporter(CodeMode codeMode)
        {
            var targetName = "cs";
            switch (codeMode)
            {
                case CodeMode.Client:
                    targetName = "c";
                    break;
                case CodeMode.Server:
                    targetName = "s";
                    break;
                default:
                    targetName = "cs";
                    break;
            }
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            string cmd = "../../Tools/Luban/gen_code_all.bat";
#endif
            var cmdPath = Application.dataPath + "/" + cmd;
            cmd = Path.GetFullPath(cmdPath);
            ShellHelper.CmdEx(cmd, out var result, true, "");
            Log.Debug(result);

            string clientProtoDir = "../UnityDemo/Assets/Bundles/Config";
            FileHelper.CopyDirectory($"../Config/Excel/{targetName}", clientProtoDir);
            FileHelper.CleanDirectory(clientProtoDir + "/StartConfig/Benchmark");
            FileHelper.CleanDirectory(clientProtoDir + "/StartConfig/Release");
            FileHelper.CleanDirectory(clientProtoDir + "/StartConfig/RouterTest");
        }
    }
}