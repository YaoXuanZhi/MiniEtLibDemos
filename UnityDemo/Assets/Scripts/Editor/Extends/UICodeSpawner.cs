using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using ET;
using NUnit.Framework;
public partial class UICodeSpawner
{
    private const string Generate = "Generate";
    static string panelName = "";
	static string HotfixViewUIDir
	{
		get
		{
			return Application.dataPath + "/Scripts/Codes/HotfixView/Client/GameRes/UI/";
		}
	}

    static string ModelViewUIDir
    {
        get
        {
            return Application.dataPath + "/Scripts/Codes/ModelView/Client/GameRes/UI/";
        }
    }

    public static string FirstToUpper(string str)
    {
        if (str == null)
        {
            return null;
        }

        if (str.Length > 1)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        return str.ToUpper();
    }

    /// <summary>
    /// UI 名字到标识符名, 用来生成回调名字
    /// 例如 ui_name_abc => UiNameAbc
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string UINameToIdentifierName(string name)
    {
        string[] keys = name.Split('_');
        string ret = "";
        for (int j = 0; j < keys.Length; j++)
        {
            string keyEle = FirstToUpper(keys[j]);
            ret += keyEle;
        }
        return ret;
    }


    public static void ShowNotification(string tips)
    {
        var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        game?.ShowNotification(new GUIContent($"{tips}"));
    }

    static public void SpawnUICode(GameObject gameObject, string name)
	{
		panelName = name;
		if (null == gameObject)
		{
			Debug.LogError("UICode Select GameObject is null!");
            ShowNotification("UICode Select GameObject is null!");
            return;
		}

        SpawnPnlCode(gameObject);
    }

    static public void SpawnPnlCode(GameObject gameObject)
    {
        //xxxEvent（只生成一次）
        if (gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui"))
        {
            SpawnCodeForPnlUIEvent(gameObject);
        }
        //xxxComponentSystem绑定（可多次生成，可更新绑定内容） 
        SpawnCodeForPnlSystemBinder(gameObject);
        //xxxComponentSystem逻辑（只生成一次） 
        SpawnCodeForPnlSystemEx(gameObject);

        //xxxComponent（可多次生成，可更新绑定内容） 
        SpawnCodeForPnlComponent(gameObject);
        //xxxComponent Partial类（只生成一次） 
        SpawnCodeForPnlPartialComponent(gameObject);
        //xxxEventType（可多次生成，可更新绑定内容） 
        //SpawnCodeForPnlEventType(gameObject);
       
        AssetDatabase.Refresh();

        //提示
        ShowNotification("Generate Code Success");
    }

    static void SpawnCodeForPnlUIEvent(GameObject gameObject)
    {
        if (!panelName.ToLower().StartsWith("pnl") && !panelName.ToLower().StartsWith("ui"))
            return;

        string strPnlName = panelName;
        string strFileDir = HotfixViewUIDir + strPnlName + "/" + Generate;
    
        if (!System.IO.Directory.Exists(strFileDir))
        {
            System.IO.Directory.CreateDirectory(strFileDir);
        }
    
        ReferenceCollector collector = gameObject.GetComponent<ReferenceCollector>();
    
        //xxxEvent
        string strFileName = string.Format("{0}Event", strPnlName);
        string strFilePath = strFileDir + "/" + strFileName + "Gen.cs";
        // if (File.Exists(strFilePath))
        // {
        //     Debug.LogErrorFormat("该文件只生成一次！名字：{0}", strFileName);
        //     return;
        // }
    
        //using 库
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using System.Collections;")
                  .AppendLine("using System.Collections.Generic;")
                  .AppendLine("using System;")
                  .AppendLine("using UnityEngine;")
                  .AppendLine("using UnityEngine.UI;")
                  .AppendLine("using ET.Client;");
        strBuilder.AppendLine("\r\n");
    
        strBuilder.AppendLine("namespace ET.Client");
        strBuilder.AppendLine("{");
    
        string space = "\t";
        //UIEvent标签
        strBuilder.AppendFormat(space + "[UIEvent(UIType.{0})]\r\n", strPnlName);
    
        //类名
        strBuilder.AppendFormat(space + "public class {0} : AUIEvent\r\n", strFileName);
        strBuilder.AppendLine(space + "{");
    
        //Create
        space = "\t\t";
        strBuilder.AppendLine(space + "public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)");
        strBuilder.AppendLine(space + "{");
        // strBuilder.AppendLine(space + "\t" + "UI ui = uiComponent.Get(uiType);");
        // strBuilder.AppendLine(space + "\t" + $"ui.AddComponent<{strPnlName}Component>();");
        
        strBuilder.AppendLine(space + "\t" + $"await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.{strPnlName}.StringToAB());");
        strBuilder.AppendLine(space + "\t" + $"GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.{strPnlName}.StringToAB(), UIType.{strPnlName});");
        strBuilder.AppendLine(space + "\t" + $"GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));");
        strBuilder.AppendLine(space + "\t" + $"UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.{strPnlName}, gameObject);");
        strBuilder.AppendLine(space + "\t" + $"var pnl = ui.AddComponent<{strPnlName}Component>();");
        strBuilder.AppendLine(space + "\t" + $"pnl.OnShow();");
        strBuilder.AppendLine(space + "\t" + $"return ui;");
        strBuilder.AppendLine(space + "}");
    
        //Remove
        space = "\t\t";
        strBuilder.AppendLine();
        strBuilder.AppendLine(space + "public override void OnRemove(UIComponent uiComponent)");
        strBuilder.AppendLine(space + "{");
        //strBuilder.AppendLine(space + "\t" + $"ResourcesComponent.Instance.UnloadBundle(UIType.{strPnlName}.StringToAB());");
        strBuilder.AppendLine(space + "}");
        
        //OnShow
        space = "\t\t";
        strBuilder.AppendLine();
        strBuilder.AppendLine(space + "public override void OnShow(UIComponent uiComponent, string uiType, params object[] args)");
        strBuilder.AppendLine(space + "{");
        strBuilder.AppendLine(space + "\t" + string.Format($"{strPnlName}Component sample = uiComponent.GetUI<{strPnlName}Component>(uiType);"));
        strBuilder.AppendLine(space + "\t" + "sample.OnShow(args);");
        strBuilder.AppendLine(space + "}");
        
        //OnHide
        space = "\t\t";
        strBuilder.AppendLine();
        strBuilder.AppendLine(space + "public override void OnHide(UIComponent uiComponent, string uiType)");
        strBuilder.AppendLine(space + "{");
        strBuilder.AppendLine(space + "\t" + string.Format($"{strPnlName}Component sample = uiComponent.GetUI<{strPnlName}Component>(uiType);"));
        strBuilder.AppendLine(space + "\t" + "sample.OnHide();");
        strBuilder.AppendLine(space + "}");

        //类结尾
        strBuilder.AppendLine("\t}");
        strBuilder.AppendLine("}");
    
        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    static void SpawnCodeForPnlComponent(GameObject gameObject)
	{
        string strPnlName = panelName;
        string strFileDir = ModelViewUIDir + strPnlName + "/" + Generate;

        if (!System.IO.Directory.Exists(strFileDir))
        {
            System.IO.Directory.CreateDirectory(strFileDir);
        }

        ReferenceCollector collector = gameObject.GetComponent<ReferenceCollector>();

        //xxxComponentSystem
        string strFileName = string.Format("{0}Component", strPnlName);
        string strFilePath = strFileDir + "/" + strFileName + "Gen.cs";

        //using 库
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using System.Collections;")
                  .AppendLine("using System.Collections.Generic;")
                  .AppendLine("using System;")
                  .AppendLine("using UnityEngine;")
                  .AppendLine("using UnityEngine.UI;");
        if (collector.IsExistComponent(CollectorType.CompositeToggle))
        {
            strBuilder.AppendLine("using Mobcast.Coffee.Toggles;");            
        }
        strBuilder.AppendLine("\r\n");

        strBuilder.AppendLine("namespace ET.Client");
        strBuilder.AppendLine("{");

        string space = "\t";
        //ComponentOf标签
        if (gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui"))
        {
            strBuilder.AppendFormat(space + "[ComponentOf(typeof(UI))]\r\n");
            //类名
            strBuilder.AppendFormat(space + "public partial class {0} : Entity, IAwake, IDestroy, ILoad\r\n",
                strFileName);
        }
        else
        {
            strBuilder.AppendFormat(space + "[ComponentOf]\r\n");
            //类名
            strBuilder.AppendFormat(space + "public partial class {0} : Entity, IAwake<Transform>, IDestroy, ILoad\r\n",
                strFileName);
        }

        strBuilder.AppendLine(space + "{");

        //生成绑定对象
        space = "\t\t";

        if (!(gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui")))
        {
            strBuilder.AppendLine(space + "public Transform root;");
            
        }
        
        for (int i = 0; i < collector.data.Count; i++)
        {
            string componentName = collector.data[i].component;
            string key = collector.data[i].key;
            if (ObjectCollector.IsGameObject(componentName))
            {
                strBuilder.AppendLine(space + string.Format("public {0} {1};", "GameObject", key));
            }            
            else if (componentName == CollectorType.Number)
            {
                strBuilder.AppendLine(space + string.Format("public float {0};", key));
            }
            else if (componentName == CollectorType.Color)
            {
                strBuilder.AppendLine(space + string.Format("public Color {0};", key));
            }
            else if (ObjectCollector.IsComponent(componentName))
            {
                strBuilder.AppendLine(space + string.Format("public {0} {1};", componentName, key));
            }
        }

        strBuilder.AppendLine("\t}");
        strBuilder.AppendLine("}");

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    static void SpawnCodeForPnlPartialComponent(GameObject gameObject)
	{
        string strPnlName = panelName;
        string strFileDir = ModelViewUIDir + strPnlName;

        if (!System.IO.Directory.Exists(strFileDir))
        {
            System.IO.Directory.CreateDirectory(strFileDir);
        }

        ReferenceCollector collector = gameObject.GetComponent<ReferenceCollector>();

        //xxxComponent
        string strFileName = string.Format("{0}Component", strPnlName);
        string strFilePath = strFileDir + "/" + strFileName + ".cs";
        if (File.Exists(strFilePath))
        {
            Debug.LogErrorFormat("该文件只生成一次！名字：{0}", strFilePath);
            return;
        }

        //using 库
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using System.Collections.Generic;")
                  .AppendLine("using UnityEngine;");
        strBuilder.AppendLine("\r\n");

        strBuilder.AppendLine("namespace ET.Client");
        strBuilder.AppendLine("{");

        string space = "\t";
        //ComponentOf标签
        //strBuilder.AppendFormat(space + "[ComponentOf(typeof(UI))]\r\n");

        //类名
        strBuilder.AppendFormat(space + "public partial class {0} \r\n", strFileName);
        strBuilder.AppendLine(space + "{");
        
        strBuilder.AppendLine("\t}");
        strBuilder.AppendLine("}");

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

	static void SpawnCodeForPnlSystemBinder(GameObject gameObject)
    {
        string strPnlName  = panelName;
        string strFileDir = HotfixViewUIDir + strPnlName + "/" + Generate;
                
        if ( !System.IO.Directory.Exists(strFileDir) )
        {
	        System.IO.Directory.CreateDirectory(strFileDir);
        }

		//xxxComponentSystem
		string strFileName = string.Format("{0}ComponentSystem", strPnlName);
        string strFilePath = strFileDir + "/" + strFileName + "Gen.cs";

        ReferenceCollector collector = gameObject.GetComponent<ReferenceCollector>();
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using System.Collections;")
                  .AppendLine("using System.Collections.Generic;")
                  .AppendLine("using System;")
                  .AppendLine("using UnityEngine;")
                  .AppendLine("using UnityEngine.UI;");
        if (collector.IsExistComponent(CollectorType.CompositeToggle))
        {
            strBuilder.AppendLine("using Mobcast.Coffee.Toggles;");
        }
        strBuilder.AppendLine("\r\n");

        strBuilder.AppendLine("namespace ET.Client");
        strBuilder.AppendLine("{");

		string space = "\t";
		//FriendOf标签
		strBuilder.AppendFormat(space + "[FriendOf(typeof({0}Component))]\r\n", strPnlName);
       
		//类名
        strBuilder.AppendFormat(space + "public static partial class {0}\r\n", strFileName);
        strBuilder.AppendLine(space + "{");

        #region 绑定对象
        strBuilder.AppendFormat(space + "\t" + "public static void Binder(this {0}Component self)\r\n", strPnlName);
        strBuilder.AppendLine(space + "\t" + "{");
        if (gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui"))
        {
            strBuilder.AppendLine(space + "\t\t" + "ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();");        
        }
        else
        {
            strBuilder.AppendLine(space + "\t\t" + "ReferenceCollector rc = self.root.GetComponent<ReferenceCollector>();");        
        }
        
        for (int i = 0; i < collector.data.Count; i++)
        {
            string componentName = collector.data[i].component;
            string key = collector.data[i].key;
            if (ObjectCollector.IsGameObject(componentName))
            {
                strBuilder.AppendLine(space + "\t\t" + string.Format("self.{0} = rc.Get<GameObject>(\"{1}\");", key, key));
            }
            else if (componentName == CollectorType.Number)
            {
                strBuilder.AppendLine(space + "\t\t" + string.Format("self.{0} = rc.GetNumber(\"{1}\");", key, key));
            }
            else if (componentName == CollectorType.Color)
            {
                strBuilder.AppendLine(space + "\t\t" + string.Format("self.{0} = rc.GetColor(\"{1}\");", key, key));
            }
            else if (ObjectCollector.IsComponent(componentName))
            {
                strBuilder.AppendLine(space + "\t\t" + string.Format("self.{0} = rc.Get<{1}>(\"{2}\");", key, componentName, key));
            }
        }       
        strBuilder.AppendLine(space + "\t" + "}");
        #endregion
        
        #region AwakeSystem
        string awakeSystem = string.Format("{0}ComponentAwakeSystem", strPnlName);
        string uiComponent = string.Format("{0}Component", strPnlName);
        space = "\t\t";
        strBuilder.AppendFormat(space + "[ObjectSystem]\r\n");
        if (gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui"))
        {
            strBuilder.AppendFormat(space + "public class {0} : AwakeSystem<{1}>\r\n", awakeSystem, uiComponent);
        }
        else
        {
            strBuilder.AppendFormat(space + "public class {0} : AwakeSystem<{1}, Transform>\r\n", awakeSystem, uiComponent);
        }
        strBuilder.AppendLine(space + "{");

        //Awake函数
        if (gameObject.name.ToLower().StartsWith("pnl") || gameObject.name.ToLower().StartsWith("ui"))
        {
            strBuilder.AppendFormat(space + "\tprotected override void Awake({0}Component self)\r\n", strPnlName);
            strBuilder.AppendLine(space + "\t{");
        }
        else
        {
            strBuilder.AppendFormat(space + "\tprotected override void Awake({0}Component self, Transform root)\r\n", strPnlName);
            strBuilder.AppendLine(space + "\t{");
            strBuilder.AppendLine(space + "\t\t" + "self.root = root;");
        }
        strBuilder.AppendLine(space + "\t\t" + "self.Binder();");
        strBuilder.AppendLine(space + "\t\t" + "self.AddListener();");
        strBuilder.AppendLine(space + "\t\t" + "self.OnInit();");
        //Awake函数结尾
        strBuilder.AppendLine(space + "\t}");

        strBuilder.AppendLine(space + "}");
        #endregion
        
        #region DestroySystem
        space = "\t\t";
        strBuilder.AppendLine();
        string destroySystem = string.Format("{0}ComponentDestroySystem", strPnlName);
        strBuilder.AppendFormat(space + "[ObjectSystem]\r\n");
        strBuilder.AppendFormat(space + "public class {0} : DestroySystem<{1}>\r\n", destroySystem, uiComponent);
        strBuilder.AppendLine(space + "{");

        //移除事件
        strBuilder.AppendFormat(space + "\tprotected override void Destroy({0}Component self)\r\n", strPnlName);
        strBuilder.AppendLine(space + "\t" + "{");
        strBuilder.AppendLine(space + "\t\t" + "self.RemoveListener();");
        strBuilder.AppendLine(space + "\t\t" + "self.OnDispose();");
        strBuilder.AppendLine(space + "\t" + "}");

        strBuilder.AppendLine(space + "}");
        strBuilder.AppendLine();
        #endregion
        
        #region LoadSystem
        space = "\t\t";
        strBuilder.AppendLine();
        string loadSystem = string.Format("{0}ComponentLoadSystem", strPnlName);
        strBuilder.AppendFormat(space + "[ObjectSystem]\r\n");
        strBuilder.AppendFormat(space + "public class {0} : LoadSystem<{1}>\r\n", loadSystem, uiComponent);
        strBuilder.AppendLine(space + "{");

        //重载
        strBuilder.AppendFormat(space + "\tprotected override void Load({0}Component self)\r\n", strPnlName);
        strBuilder.AppendLine(space + "\t" + "{");
        strBuilder.AppendLine(space + "\t\t" + "self.Load();");
        strBuilder.AppendLine(space + "\t" + "}");

        strBuilder.AppendLine(space + "}");
        strBuilder.AppendLine();
        #endregion

        #region Load函数
        space = "\t\t";
        strBuilder.AppendFormat(space + "public static void Load(this {0}Component self)\r\n", strPnlName);
        strBuilder.AppendLine(space + "{");
        strBuilder.AppendLine(space + "\t" + "self.RemoveListener();");
        strBuilder.AppendLine(space + "\t" + "self.AddListener();");
        strBuilder.AppendLine(space + "\t" + "self.OnShow();");
        strBuilder.AppendLine(space + "}");
        strBuilder.AppendLine();
        #endregion

        //类结尾
        strBuilder.AppendLine("\t}");
        strBuilder.AppendLine("}");

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }
}

