using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// REPL 控制台组件，挂载在 Root.Scene 上。
    /// 数据定义在 Model 层，逻辑在 Hotfix 层，支持热重载。
    /// ILoad 接口确保热更新后重新发现命令处理器。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ReplComponent : Entity, IAwake, IDestroy, ILoad
    {
        /// <summary>当前选中的 Entity InstanceId</summary>
        public long SelectedEntityId;

        /// <summary>选中的显示标签</summary>
        public string SelectedLabel = "";

        /// <summary>命令历史</summary>
        public List<string> History = new();

        /// <summary>是否正在运行</summary>
        public bool Running;

        /// <summary>反射方法缓存：Type => MethodEntry[]</summary>
        public ConcurrentDictionary<Type, ReplMethodEntry[]> MethodCache = new();

        /// <summary>命令处理器字典：command => handler（热更新时会重新加载）</summary>
        public Dictionary<string, IReplCommandHandler> Handlers = new();

        /// <summary>命令描述字典：command => description</summary>
        public Dictionary<string, string> CommandDescriptions = new();
    }

    public sealed class ReplMethodEntry
    {
        public string Name = "";
        public System.Reflection.MethodInfo Info = null!;
        public System.Reflection.ParameterInfo[] Parameters = Array.Empty<System.Reflection.ParameterInfo>();
        public bool IsAsync;
    }
}
