namespace ET
{
    public interface IConsoleHandler
    {
        ETTask Run(ModeContext context, string content);
    }
}