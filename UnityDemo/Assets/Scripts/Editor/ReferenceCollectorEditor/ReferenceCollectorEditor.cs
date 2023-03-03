using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

public class ReferenceCollectorWindow : EditorWindow
{
    private static List<GameObject> selectionList = new List<GameObject>();
    private static List<ReferenceCollector> collectorList = new  List<ReferenceCollector>();
    Vector2 scrollPos;

    [MenuItem("GameObject/UI节点批量绑定", false, -2)]
    static public void CreateNewCode()
    {
        selectionList.Clear();
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            selectionList.Add(Selection.objects[i] as GameObject);
        }
        GameObject pnlRoot = null;
        for (int i = 0; i < selectionList.Count; i++)
        {
            ReferenceCollector coll = selectionList[i].GetComponentInParent<ReferenceCollector>();
            // if (coll.name.StartsWith("pnl"))
            {
                pnlRoot = coll.gameObject;
                // break;
            }
        }
        // if (pnlRoot == null)
        // {
        //     Debug.LogError("没有找到带pnl开头的父节点");
        //      return;
        // }
        ReferenceCollector[] collectors = pnlRoot.GetComponentsInChildren<ReferenceCollector>();
        collectorList.Clear();
        collectorList.AddRange(collectors);
        // ReferenceCollector collector = pnlRoot.GetComponentInChildren<ReferenceCollector>();
        // collectorList.Clear();
        // collectorList.Add(collector);
        
        EditorWindow window = GetWindow(typeof(ReferenceCollectorWindow));
        window.titleContent = new GUIContent("UI节点批量绑定");
    }

    private void OnGUI()
    {
        if (collectorList == null)
        {
            return;
        }

        if (collectorList.Count == 0)
        {
            return;
        }
        
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(300), GUILayout.Height(800)))
        {
            scrollPos = scrollView.scrollPosition;
            for (int i = 0; i < collectorList.Count; i++)
            {
                ReferenceCollector collector = collectorList[i];
                if (GUILayout.Button(string.Format($"绑定到节点{collector.name}的ReferenceCollector"), GUILayout.Width(300), GUILayout.Height(50)))
                {
                    for (int j = 0; j < selectionList.Count; j++)
                    {
                        GameObject selectionGo = selectionList[j];
                        collector.Add(selectionGo.name, selectionGo);    
                    }
                }
            }
        }
    }
}

//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorEditor: Editor
{

    private ReferenceCollector referenceCollector;
    private Object inputObj;
    private string searchKey = "";
    ReorderableList reorderableList;

    //输入在textfield中的字符串
    private string SearchKey
	{
		get
		{
			return searchKey;
		}
		set
		{
			if (searchKey != value)
			{
				searchKey = value;
				inputObj = referenceCollector.Get<Object>(SearchKey);
			}
		}
	}
    
    

	private void OnEnable()
	{
        //将被选中的gameobject所挂载的ReferenceCollector赋值给编辑器类中的ReferenceCollector，方便操作
        referenceCollector = (ReferenceCollector) target;
		reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("data"));
		reorderableList.drawElementCallback = OnDrawElementCallback;		
		reorderableList.drawHeaderCallback = OnDrawHeaderCallback;
        reorderableList.onAddCallback = OnAddCallback;
        reorderableList.onRemoveCallback = OnRemoveCallback;
    }

    public Rect[] GetElementRects(Rect r)
    {
        Rect[] rects = new Rect[6];
        float remainingWidth = r.width;
        float orderWidth = 30;
        float delWidth = 30;
        float registWidth = 40;
        float offset = 4;
        float contentWidth = Mathf.FloorToInt((remainingWidth - orderWidth - registWidth - delWidth - offset * 5) / 3);
        int colIndex = 0;
        float x = r.x;
        //ID
        rects[colIndex] = new Rect(x, r.y, orderWidth, r.height);
        colIndex++;
        x += orderWidth + offset;
        //名字
        rects[colIndex] = new Rect(x, r.y, contentWidth, r.height);
        colIndex++;
        x += contentWidth + offset;
        //引用
        rects[colIndex] = new Rect(x, r.y, contentWidth, r.height);
        colIndex++;
        x += contentWidth + offset;
        //类型
        rects[colIndex] = new Rect(x, r.y, contentWidth, r.height);
        colIndex++;
        x += contentWidth + offset;
        //自动注册
        rects[colIndex] = new Rect(x, r.y, registWidth, r.height);
        colIndex++;
        x += registWidth + offset;
        //删除
        rects[colIndex] = new Rect(x, r.y, delWidth, r.height);
        return rects;
    }

    private bool IsRepeat(string key)
    {
        int count = 0;
        for (int i = 0; i < referenceCollector.data.Count; i++)
        {
            var elementData = referenceCollector.data[i];
            if (elementData.key.Equals(key))
            {
                count++;
            }
        }
        return count > 1;
    }

    private void OnDrawElementCallback(Rect r, int index, bool selected, bool focused)
	{
        if (index >= reorderableList.serializedProperty.arraySize)
        {
            return;
        }

        GUI.backgroundColor = Color.white;
        var elementData = referenceCollector.data[index];
        if (IsRepeat(elementData.key) && !string.IsNullOrEmpty(elementData.key))
        {
            GUI.backgroundColor = Color.red;
        }

        var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
        var refComponent = element.FindPropertyRelative("component");        
        var refKey = element.FindPropertyRelative("key");

        //ID
        Rect[] rects = GetElementRects(r);
        GUI.Label(rects[0], index.ToString());        

        //名字
        var objTitle = new GUIContent("");
        EditorGUI.PropertyField(rects[1], refKey, objTitle);

        //引用
        Rect refRect = rects[2];
        if (refComponent.stringValue == CollectorType.Number)
        {
            EditorGUI.PropertyField(refRect, element.FindPropertyRelative("refValue"), objTitle);
        }
        else if (refComponent.stringValue == CollectorType.Color)
        {
            EditorGUI.PropertyField(refRect, element.FindPropertyRelative("refColor"), objTitle);
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            SerializedProperty refObj = element.FindPropertyRelative("gameObject");
            EditorGUI.PropertyField(refRect, refObj, objTitle);
            if (EditorGUI.EndChangeCheck())
            {
                var obj = refObj.objectReferenceValue;
                GameObject go = null;
                if (obj is GameObject)
                {
                    go = obj as GameObject;
                }
                else if (obj is Component)
                {
                    go = (obj as Component).gameObject;
                }
                if (go != null)
                {
                    var type = refComponent.stringValue;
                    type = ObjectCollector.CheckDefultType(go, type);
                    SetObjectType(element, go, type);
                }
            }
        }

        //类型
        var objs = ObjectCollector.GetCollectors();
        var lastSelectIndex = ObjectCollector.GetIndexByName(refComponent.stringValue);
        var selectIndex = EditorGUI.Popup(rects[3], lastSelectIndex, objs);
        if (lastSelectIndex != selectIndex)
        {
            GameObject go = element.FindPropertyRelative("go").objectReferenceValue as GameObject;
            SetObjectType(element, go, objs[selectIndex]);
        }

        //注册
        if (refComponent.stringValue == CollectorType.Button
            || refComponent.stringValue == CollectorType.UIButton)
        {
            var isAutoRegist = element.FindPropertyRelative("isAutoRegist");
            EditorGUI.PropertyField(rects[4], isAutoRegist, objTitle);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            var isAutoRegist = element.FindPropertyRelative("isAutoRegist");
            isAutoRegist.boolValue = false;
            EditorGUI.PropertyField(rects[4], isAutoRegist, objTitle);
            EditorGUI.EndDisabledGroup();
        }

        //删除
        if (GUI.Button(rects[5], "×"))
        {
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);
        };
    }
    
    private void SetObjectType(SerializedProperty property, GameObject go, string type)
    {
        var component = property.FindPropertyRelative("component");
        var obj = property.FindPropertyRelative("gameObject");
        component.stringValue = type;

        if (type == CollectorType.GameObject)
        {
            obj.objectReferenceValue = go;
        }
        else
        {
            obj.objectReferenceValue = go.GetComponent(type);
        }

        string refName = property.FindPropertyRelative("key").stringValue;
        if (string.IsNullOrEmpty(refName))
        {
            property.FindPropertyRelative("key").stringValue = go.name;
        }

        property.FindPropertyRelative("go").objectReferenceValue = go;
    }

    public void OnAddCallback(ReorderableList list)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("添加数字参数"), false, AddNumber);
        menu.AddItem(new GUIContent("添加颜色参数"), false, AddColor);
        menu.ShowAsContext();
    }

    public void AddNumber()
    {
		referenceCollector.data.Add(new ReferenceCollectorData("New Number", CollectorType.Number));
    }

    public void AddColor()
    {
		referenceCollector.data.Add(new ReferenceCollectorData("New Color", CollectorType.Color));
    }

    private void OnRemoveCallback(ReorderableList list)
    {
		reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
    }

    private void OnDrawHeaderCallback(Rect headerRect)
    {
        headerRect.xMin += 14; // 忽略拖拽按钮的宽度
        headerRect.y++;
        headerRect.height = 15;

        Rect[] rects = GetElementRects(headerRect);
        int col = 0;
        string[] names = {
            "ID",
            "名字",
            "引用",
            "类型",
            "注册",
            "删除",
        };
        for (int i = 0; i < rects.Length; i++)
        {
            GUI.Label(rects[col], names[i], EditorStyles.label);
            col++;
        }
    }

    public override void OnInspectorGUI()
	{
        EditorGUI.BeginChangeCheck();
        
        //引用列表
		reorderableList.DoLayoutList();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal();
        //可以在编辑器中对searchKey进行赋值，只要输入对应的Key值，就可以点后面的删除按钮删除相对应的元素
        SearchKey = EditorGUILayout.TextField(SearchKey);
        //添加的可以用于选中Object的框，这里的object也是(UnityEngine.Object
        //第三个参数为是否只能引用scene中的Object
        EditorGUILayout.ObjectField(inputObj, typeof(Object), false);
        if (GUILayout.Button("删除"))
        {
            referenceCollector.Remove(SearchKey);
            inputObj = null;
        }
        EditorGUILayout.EndHorizontal();

        //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        var eventType = Event.current.type;        
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            // Show a copy icon on the drag
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var o in DragAndDrop.objectReferences)
                {
                    GameObject tempGo = o as GameObject;
                    AddReference(tempGo);
                }
            }

            Event.current.Use();
        }

        EditorGUILayout.BeginHorizontal();
        if (referenceCollector.gameObject.name.StartsWith("pnl")
            || referenceCollector.gameObject.name.StartsWith("UI")
            || referenceCollector.gameObject.name.StartsWith("ext")
            || referenceCollector.gameObject.name.StartsWith("layout")
            || referenceCollector.gameObject.name.StartsWith("nego"))
        {
            if (GUILayout.Button("生成代码"))
            {
                string panelName = "";
                string goName = referenceCollector.gameObject.name;
                string[] uiName = goName.Split('_');
                if (uiName.Length > 0)
                {
                    for (int i = 0; i < uiName.Length; i++)
                    {
                        panelName += UICodeSpawner.FirstToUpper(uiName[i]);
                    }
                }
                else
                {
                    panelName = goName;
                }
                UICodeSpawner.SpawnUICode(referenceCollector.gameObject, panelName);
            }
        }        
        if (GUILayout.Button("清空列表"))
        {
            if (EditorUtility.DisplayDialog("提示", "确定清空引用列表吗？", "确定", "取消"))
            {
                referenceCollector.Clear();
            }            
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }

    //添加元素，具体知识点在ReferenceCollector中说了
    private void AddReference(GameObject obj)
	{
        ReferenceCollectorData collectorData = new ReferenceCollectorData(obj.name, obj);
        collectorData.SetObjectType(ObjectCollector.GetDefultType(obj));

        referenceCollector.data.Add(collectorData);
        EditorUtility.SetDirty(serializedObject.targetObject);
    }
}
