using System;
using System.Collections.Generic;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

//使其能在Inspector面板显示，并且可以被赋予相应值
[Serializable]
public class ReferenceCollectorData
{
	public string key;
    //Object并非C#基础中的Object，而是 UnityEngine.Object
    public Object gameObject;

	public string component = "";

    public Color refColor = Color.white;
    public float refValue;
    public bool isAutoRegist = true;

    public ReferenceCollectorData(string key, GameObject go)
    {
        this.key = key;
        this.go = go;
    }

    public ReferenceCollectorData(string key, string component)
    {
        this.key = key;
        this.component = component;
    }

    public GameObject go;
    public GameObject GetGo()
    {
        if (go != null)
        {
			return go;
        }
               
		if (gameObject is GameObject)
        {
            go = (gameObject as GameObject);
			return go;
        }

        if (gameObject is Component)
        {
            go = (gameObject as Component).gameObject;
            return go;
        }

        return null;
    }

    public Object GetReferenceObj()
    {
		return gameObject;
	}

    public void SetObjectType(string component)
    {
        this.component = component;
        if (GetGo() == null)
        {
            return;
        }

        if (ObjectCollector.IsGameObject(component))
        {
            gameObject = GetGo();
            refValue = 0;
			return;
        }

        if (ObjectCollector.IsComponent(component))
        {
            gameObject = GetGo().GetComponent(component);
            refValue = 0;
			return;
        }
    }
}

//继承IComparer对比器，Ordinal会使用序号排序规则比较字符串，因为是byte级别的比较，所以准确性和性能都不错
public class ReferenceCollectorDataComparer: IComparer<ReferenceCollectorData>
{
	public int Compare(ReferenceCollectorData x, ReferenceCollectorData y)
	{
		return string.Compare(x.key, y.key, StringComparison.Ordinal);
	}
}

//继承ISerializationCallbackReceiver后会增加OnAfterDeserialize和OnBeforeSerialize两个回调函数，如果有需要可以在对需要序列化的东西进行操作
//ET在这里主要是在OnAfterDeserialize回调函数中将data中存储的ReferenceCollectorData转换为dict中的Object，方便之后的使用
//注意UNITY_EDITOR宏定义，在编译以后，部分编辑器相关函数并不存在
public class ReferenceCollector: MonoBehaviour, ISerializationCallbackReceiver
{
    //用于序列化的List
	public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();
    //Object并非C#基础中的Object，而是 UnityEngine.Object
    private readonly Dictionary<string, ReferenceCollectorData> dict = new Dictionary<string, ReferenceCollectorData>();

    private void Awake()
    {
        foreach (ReferenceCollectorData referenceCollectorData in data)
        {
            if (!dict.ContainsKey(referenceCollectorData.key))
            {
                dict.Add(referenceCollectorData.key, referenceCollectorData);
            }
        }
    }

#if UNITY_EDITOR
	//添加新的元素
	public void Add(string key, GameObject obj)
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);

		ReferenceCollectorData collectorData = new ReferenceCollectorData(obj.name, obj);
		collectorData.SetObjectType(ObjectCollector.GetDefultType(obj));
		data.Add(collectorData);
		
		//应用与更新
		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
	
    //删除元素，知识点与上面的添加相似
    public void Remove(string key)
	{
		int i;
		for (i = 0; i < data.Count; i++)
		{
			if (data[i].key == key)
			{
				break;
			}
		}
	}

	public void Clear()
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        //根据PropertyPath读取prefab文件中的数据
        //如果不知道具体的格式，可以直接右键用文本编辑器打开，搜索data就能找到
        var dataProperty = serializedObject.FindProperty("data");
		dataProperty.ClearArray();
		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
#endif

	/// <summary>
	/// 根据key的获得gameobject
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public Object GetObject(string key)
	{
		if (dict.TryGetValue(key, out var collectorData))
		{
			return collectorData.gameObject;
		}
		return null;
	}

	//使用泛型返回对应key的gameobject
    public T Get<T>(string key) where T : class
	{
		if (dict.TryGetValue(key, out var collectorData))
		{
			return collectorData.gameObject as T;
		}
		return null;
	}

    public float GetNumber(string key, float defaultValue = 0)
    {
        if (dict.TryGetValue(key, out var collectorData))
        {
            return collectorData.refValue;
        }
        return defaultValue;
    }

    public Color GetColor(string key)
    {
        if (dict.TryGetValue(key, out var collectorData))
        {
            return collectorData.refColor;
        }
        return Color.white;
    }

	public void OnBeforeSerialize()
	{
	}

    //在反序列化后运行
	public void OnAfterDeserialize()
	{
		dict.Clear();
		foreach (ReferenceCollectorData referenceCollectorData in data)
		{
			if (!dict.ContainsKey(referenceCollectorData.key))
			{
				dict.Add(referenceCollectorData.key, referenceCollectorData);
			}
		}
	}

	public bool IsExistComponent(string component)
	{
		for (int i = 0; i < data.Count; i++)
		{
			if (data[i].component == component)
			{
				return true;
			}
		}
		return false;
	}
}