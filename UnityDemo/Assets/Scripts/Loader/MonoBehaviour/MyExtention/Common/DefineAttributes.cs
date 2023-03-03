using System;
using System.Collections.Generic;
using System.Security.AccessControl;

using UnityEngine;

/// <summary>
/// 标记不生成代码的类/方法/属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class NoGenBoltCodeAttribute : Attribute
{
}

/// <summary>
/// 标记生成代码的类/方法/属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class GenBoltCodeAttribute : Attribute
{
}


/// <summary>
/// 生成自定义组件代码
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GenComponentBoltCodeAttribute : GenBoltCodeAttribute
{
}

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttributeEx : PropertyAttribute
{
    public string label;
    public new int[] order = new int[0];
    public EnumLabelAttributeEx(string label)
    {
        this.label = label;
    }

    public EnumLabelAttributeEx(string label, params int[] order)
    {
        this.label = label;
        this.order = order;
    }

}

/// <summary>
/// 此标签标识的Action类会在编辑器的显示框进行过滤
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HideInSearchActionAttribute : Attribute
{
    public HideInSearchActionAttribute() { }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class HandleEventAttribute : Attribute
{
	public string Evts_1;
	public string Evts_2;
	public string Evts_3;
	public string Evts_4;
	public string Evts_5;
	public string Evts_6;
	public string Evts_7;
	public string Evts_8;
	public string Evts_9;
	public string Evts_10;

	public HandleEventAttribute(string evts)
	{
		Evts_1 = evts;
	}
	public HandleEventAttribute(string evts, string evts1)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4, string evts5)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
		Evts_6 = evts5;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4, string evts5, string evts6)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
		Evts_6 = evts5;
		Evts_7 = evts6;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4, string evts5, string evts6, string evts7)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
		Evts_6 = evts5;
		Evts_7 = evts6;
		Evts_8 = evts7;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4, string evts5, string evts6, string evts7, string evts8)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
		Evts_6 = evts5;
		Evts_7 = evts6;
		Evts_8 = evts7;
		Evts_9 = evts8;
	}
	public HandleEventAttribute(string evts, string evts1, string evts2, string evts3, string evts4, string evts5, string evts6, string evts7, string evts8, string evts9)
	{
		Evts_1 = evts;
		Evts_2 = evts1;
		Evts_3 = evts2;
		Evts_4 = evts3;
		Evts_5 = evts4;
		Evts_6 = evts5;
		Evts_7 = evts6;
		Evts_8 = evts7;
		Evts_9 = evts8;
		Evts_10 = evts9;
	}
	public List<string> GetEvts()
	{
		List<string> evts = new();
		if (Evts_1 != null)
			evts.Add(Evts_1);
		if (Evts_2 != null && !evts.Contains(Evts_2))
			evts.Add(Evts_2);
		if (Evts_3 != null && !evts.Contains(Evts_3))
			evts.Add(Evts_3);
		if (Evts_4 != null && !evts.Contains(Evts_4))
			evts.Add(Evts_4);
		if (Evts_5 != null && !evts.Contains(Evts_5))
			evts.Add(Evts_5);
		if (Evts_6 != null && !evts.Contains(Evts_6))
			evts.Add(Evts_6);
		if (Evts_7 != null && !evts.Contains(Evts_7))
			evts.Add(Evts_7);
		if (Evts_8 != null && !evts.Contains(Evts_8))
			evts.Add(Evts_8);
		if (Evts_9 != null && !evts.Contains(Evts_9))
			evts.Add(Evts_9);
		if (Evts_10 != null && !evts.Contains(Evts_10))
			evts.Add(Evts_10);
		return evts;
	}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ActionCategoryAttribute : Attribute
{
    public string category;
    public ActionCategoryAttribute(string category)
    {
        this.category = category;
    }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ActionIconAttribute : Attribute
{
    public string IconPath { get { return mIconPath; } }
    public readonly string mIconPath;
    public ActionIconAttribute(string iconPath) { mIconPath = iconPath; }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ExtDataDescAttribute : Attribute
{
    public string Description { get { return mDescription; } }
    public readonly string mDescription;

    public ExtDataDescAttribute(string description) { mDescription = description; }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ExtDataExampleAttribute : Attribute
{
    public string Example { get { return mExample; } }
    public readonly string mExample;
    public ExtDataExampleAttribute(string formatDesc) { mExample = formatDesc; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]

public class TimelineConfigAttribute : Attribute
{
	public string title;

	public TimelineConfigAttribute(string title)
	{
		this.title = title;
	}
}
