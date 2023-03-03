using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UICodeSpawnerWindow : EditorWindow
{
    private static GameObject selectionGo;
    private static string panelName;
    private static GUIStyle redTextStyle; 

    public static void ShowWindow(GameObject selectionObj)
    {
        selectionGo = selectionObj;        
        GetWindow(typeof(UICodeSpawnerWindow));
    }
    
    private void OnGUI()
    {
        if (redTextStyle == null)
        {
            redTextStyle = new GUIStyle();
            redTextStyle.normal.textColor = Color.red;
        }

        string goName = selectionGo != null ? selectionGo.name : "";
        string[] uiName = goName.Split('_');
        if (uiName.Length > 0)
        {
            panelName = "";
            for (int i = 0; i < uiName.Length; i++)
            {
                panelName += UICodeSpawner.FirstToUpper(uiName[i]);
            }
        }
        else
        {
            panelName = goName;
        }

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("UI面板");
            selectionGo = EditorGUILayout.ObjectField(selectionGo, typeof(GameObject), true) as GameObject;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("面板名称(UIType)");
            EditorGUILayout.LabelField(panelName, redTextStyle);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("生成代码"))
        {
            if (selectionGo == null)
            {
                Debug.LogError("必须指定一个UI面板！");
                return;
            }

            if (selectionGo.GetComponent<ReferenceCollector>() == null)
            {
                Debug.LogError("UI面板必须绑定脚本ReferenceCollector");
                return;
            }

            UICodeSpawner.SpawnUICode(selectionGo, panelName);

            AssetDatabase.Refresh();
        }
    }
}
