using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mobcast.Coffee.Toggles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ET
{
    //使其能在Inspector面板显示，并且可以被赋予相应值
    [Serializable]
    public class ExCompositeToggleData
    {
    	public string key;
        //Object并非C#基础中的Object，而是 UnityEngine.Object
        public CompositeToggle toggle;
        private int[] stateValues;

        private int cacheState = 0;
        public int[] StateValues
        {
            get
            {
                if (cacheState == stateValue)
                {
                    return stateValues;
                }
                stateValues = CompositeUtil.CalculationState(stateValue, toggle.count);
                cacheState = stateValue;
                return stateValues;
            }
        }
        
        public int stateValue;
        public string type = "";
        
        public ExCompositeToggleData(string key, CompositeToggle tog)
        {
            this.key = key;
            toggle = tog;

            stateValues = new int[tog.count];
            for (int i = 0; i < tog.count; i++)
            {
                stateValues[i] = i;
            }
        }
    }

    public enum EnumOperatorType
    {
        And = 0,
        Or = 1,
    }
    
    [ExecuteInEditMode]
    public class ExCompositeToggle : MonoBehaviour
    {
        //用于序列化的List
        public List<ExCompositeToggleData> data = new List<ExCompositeToggleData>();
        //Object并非C#基础中的Object，而是 UnityEngine.Object
        private readonly Dictionary<string, ExCompositeToggleData> dict = new Dictionary<string, ExCompositeToggleData>();

        private Dictionary<CompositeToggle, UnityAction<CompositeToggle>> events =
            new Dictionary<CompositeToggle, UnityAction<CompositeToggle>>();

        public EnumOperatorType Operator = EnumOperatorType.Or;

        private void Awake()
        {
            Init();
        }
        
        // private ExCompositeToggle():base()
        // {
        //     Debug.LogError("1111111111111111111111");
        //     Init();
        // }

        public void Init()
        {
            if (data.Count == 0)
            {
                return;
            }
            dict.Clear();
            foreach (ExCompositeToggleData toggleData in data)
            {
                if (!dict.ContainsKey(toggleData.key))
                {
                    dict.Add(toggleData.key, toggleData);
                }

                if (toggleData.toggle == null)
                {
                    continue;
                }
                toggleData.toggle.onRefreshEvent = OnRefresh;
#if UNITY_EDITOR
                if (!toggleData.toggle.ReferenceExToggles.Contains(this))
                {
                    toggleData.toggle.ReferenceExToggles.Add(this);
                }        
#endif
            }
            Refresh();
        }

#if UNITY_EDITOR
        public void Remove(CompositeToggle toggle)
        {
            int i;
            for (i = 0; i < data.Count; i++)
            {
                if (data[i].toggle == null)
                {
                    continue;
                }
                if (data[i].toggle == toggle)
                {
                    data[i].toggle.onRefreshEvent = null;
                    events.Remove(data[i].toggle);
                    break;
                }
            }
        }
        
        public void RemoveAndSave(CompositeToggle toggle)
        {
            Remove(toggle);
            Save();
        }

        public void Clear()
        {
            events.Clear();
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].toggle == null)
                {
                    continue;
                }
                data[i].toggle.onRefreshEvent = null;
            }
        }

        private void Save()
        {
            UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
            //根据PropertyPath读取prefab文件中的数据
            var dataProperty = serializedObject.FindProperty("data");
            dataProperty.ClearArray();
            UnityEditor.EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
#endif
        
        public void OnRefresh(CompositeToggle toggle)
        {
            Refresh();
        }
        
        public void RefreshActiveState()
        {
            if (data.Count == 0)
            {
                return;
            }
            
            // int ret = data[0].stateValue;
            // int maxCount = 0;
            // for (int i = 0; i < data.Count; i++)
            // {
            //     ExCompositeToggleData togData = data[i];
            //     if (Operator == EnumOperatorType.And)
            //     {
            //         ret &= togData.stateValue;
            //     }
            //     else
            //     {
            //         ret |= togData.stateValue;
            //     }
            //
            //     if (maxCount < togData.toggle.count)
            //     {
            //         maxCount = togData.toggle.count;    
            //     }
            // }
            // int[] states = ExCompositeToggleData.CalculationState(ret, maxCount);
            bool[] retState = new bool[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                ExCompositeToggleData togData = data[i];
                if (togData.toggle == null)
                {
                    continue;
                }
                
                CompositeToggle toggle = togData.toggle;
                int[] states = togData.StateValues;
                if (togData.toggle != null && states != null)
                {
                    if (togData.toggle.indexValue < states.Length)
                    {
                        retState[i] = states[toggle.indexValue] == 1;
                    }
                }
            }
            
            //默认赋值，与操作，默认true，或操作，默认false,方便计算
            bool isShow = Operator == EnumOperatorType.And; 
            for (int i = 0; i < retState.Length; i++)
            {
                if (Operator == EnumOperatorType.And)
                {
                    //与操作，只要有一个是结果为false，就不需要显示
                    if (retState[i] == false)
                    {
                        isShow = false;
                        break;
                    }
                }
                else
                {
                    //或操作，只要有一个结果是显示的，就需要显示
                    if (retState[i] == true)
                    {
                        isShow = true;
                        break;
                    }
                }
            }
            gameObject.SetActive(isShow);
        }

        public void RefreshActiveState(int index)
        {
            if (index >= data.Count)
            {
                return;
            }
            RefreshActiveState(data[index]);
        }

        public void RefreshActiveState(ExCompositeToggleData togData)
        {
            if (togData.toggle == null)
            {
                return;
            }
            CompositeToggle toggle = togData.toggle;
            if (toggle != null)
            {
                int[] states = togData.StateValues;
                switch (toggle.valueType)
                {
                    case CompositeToggle.ValueType.Boolean:
                        break;
                    case CompositeToggle.ValueType.Index:
                        if (togData.toggle.indexValue < states.Length)
                        {
                            gameObject.SetActive(states[toggle.indexValue]==1);
                        }
                        break;
                    case CompositeToggle.ValueType.Count:
                        break;
                    case CompositeToggle.ValueType.Flag:
                        break;
                    default:
                        break;
                }
            }
        }

        public void Refresh()
        {
            for (int i = data.Count-1; i >= 0; i--)
            {
                ExCompositeToggleData togData = data[i];
                if (togData == null)
                {
                    data.RemoveAt(i);
                    continue;
                }

                if (togData.toggle == null)
                {
                    data.RemoveAt(i);
                    continue;
                }
                AddListener(togData);
            }

            RefreshActiveState();
        }

        public void AddListener(ExCompositeToggleData togData)
        {
            if (togData.toggle == null)
            {
                return;
            }
            CompositeToggle toggle = togData.toggle;
            UnityAction<CompositeToggle> changeEvent = null;
            if (events.TryGetValue(toggle, out changeEvent))
            {
                toggle.onValueChanged.RemoveListener(changeEvent);
            }
            else
            {
                changeEvent = (value) =>
                {
                    OnValueChange(value, togData);
                };
                events[toggle] = changeEvent;
            }
            toggle.onValueChanged.AddListener(changeEvent);
        }

        private void OnValueChange(CompositeToggle value, ExCompositeToggleData togData)
        {
            RefreshActiveState();
        }

        public CompositeToggle Get(string key)
        {
            if (dict.TryGetValue(key, out var collectorData))
            {
                return collectorData.toggle;
            }
            return null;
        }

        public void OnDestroy()
        {
            List<UnityAction<CompositeToggle>> tmpList = new List<UnityAction<CompositeToggle>>();
            foreach (var item in events)
            {
                if (item.Value == null)
                {
                    continue;
                }
                item.Key.onValueChanged.RemoveListener(item.Value);
                tmpList.Add(item.Value);
            }
            for (int i = tmpList.Count -1; i >= 0 ; i--)
            {
                tmpList[i] = null;
            }
            tmpList.Clear();
            events.Clear();
#if UNITY_EDITOR
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].toggle == null)
                {
                    continue;
                }
                data[i].toggle.ReferenceExToggles.Remove(this);
            }
#endif
        }
    }
}
