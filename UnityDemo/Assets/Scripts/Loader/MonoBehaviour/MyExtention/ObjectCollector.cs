using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectorType
{
    public static string LoopScrollRect = "LoopScrollRect";
    public static string LoopScrollRectMulti = "LoopScrollRectMulti";
    public static string UIButton = "UIButton";
    public static string Toggle = "Toggle";
    public static string Dropdown = "Dropdown";
    public static string InputField = "InputField";
    public static string RawImage = "RawImage";
    public static string Slider = "Slider";
    public static string Text = "Text";    
    public static string Button = "Button";
    public static string CompositeToggle = "CompositeToggle";
    public static string Image = "Image";
    public static string Canvas = "Canvas";
    public static string CanvasGroup = "CanvasGroup";
    public static string Camera = "Camera";
    public static string GameObject = "GameObject";
    public static string RectTransform = "RectTransform";
    public static string Transform = "Transform";
    public static string Number = "Number";
    public static string Color = "Color";
}

public class ObjectCollector
{
    public static string[] GetCollectors()
    {
        List<string> objs = new List<string>();
        var fileds = typeof(CollectorType).GetFields();
        foreach (var item in fileds)
        {
            if (item.FieldType == typeof(string))
            {
                string name = (string)item.GetValue(null);
                objs.Add(name);
            }
        }
        return objs.ToArray();
    }

    public static int GetIndexByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = CollectorType.GameObject;
        }
        string[] comps = GetCollectors();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == name)
            {
                return i;
            }
        }
        return 0;
    }

    public static bool IsComponent(string name)
    {
        return name != CollectorType.Number || name != CollectorType.Color;
    }

    public static bool IsGameObject(string name)
    {
        return name == CollectorType.GameObject;
    }
    
    public static string CheckDefultType(GameObject go, string defultType)
    {
        if (defultType != CollectorType.GameObject && !go.GetComponent(defultType))
        {
            defultType = GetDefultType(go);
        }
        return defultType;
    }

    public static string GetDefultType(GameObject go)
    {
        var objs = ObjectCollector.GetCollectors();
        for (int i = 0; i < objs.Length; i++)
        {
            string typeName = objs[i];
            if (typeName != CollectorType.GameObject)
            {
                if (go.GetComponent(typeName))
                {
                    return typeName;
                };
            }
        }
        return CollectorType.GameObject;
    }
}
