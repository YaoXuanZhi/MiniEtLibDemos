using UnityEditor;

namespace ET
{
    public static class ToolsEditor
    {
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            const string cmd = "../Config/Tools/Luban/gen_code_all.bat";
#endif
            ShellHelper.CmdEx(cmd, out var result, true, "");
            UnityEngine.Debug.Log(result);
        }
    }
}