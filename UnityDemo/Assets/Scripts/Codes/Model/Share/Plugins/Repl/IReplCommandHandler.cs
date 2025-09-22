namespace ET
{
    /// <summary>
    /// REPL 命令处理器接口。
    /// 实现类通过 [ReplCommand("cmd", "desc")] 注册，支持热重载。
    /// </summary>
    public interface IReplCommandHandler
    {
        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <param name="repl">REPL 组件实例，可访问选中状态、方法缓存等</param>
        /// <param name="content">用户输入的完整行内容（含命令本身）</param>
        /// <param name="args">去掉命令名之后的参数数组</param>
        ETTask Run(ReplComponent repl, string content, string[] args);
    }
}

