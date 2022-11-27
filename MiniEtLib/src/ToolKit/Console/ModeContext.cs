namespace ET
{
    [ObjectSystem]
    public class ModeContextAwakeSystem: AwakeSystem<ModeContext>
    {
        protected override void Awake(ModeContext self)
        {
            self.Mode = "";
        }
    }

    [ObjectSystem]
    public class ModeContextDestroySystem: DestroySystem<ModeContext>
    {
        protected override void Destroy(ModeContext self)
        {
            self.Mode = "";
        }
    }

    [ComponentOf(typeof(ConsoleComponent))]
    public class ModeContext: Entity, IAwake, IDestroy
    {
        public string Mode = "";
    }
}