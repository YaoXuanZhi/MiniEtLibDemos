namespace ET
{
    /// <summary>
    /// 标记一个 REPL 命令处理器。
    /// 被标记的类必须实现 IReplCommandHandler 接口。
    /// </summary>
    public class ReplCommandAttribute : BaseAttribute
    {
        /// <summary>命令名（小写），如 "list"、"select"、"reload"</summary>
        public string Command { get; }

        /// <summary>命令简短描述，用于 help 输出</summary>
        public string Description { get; }

        public ReplCommandAttribute(string command, string description = "")
        {
            this.Command = command;
            this.Description = description;
        }
    }
}

