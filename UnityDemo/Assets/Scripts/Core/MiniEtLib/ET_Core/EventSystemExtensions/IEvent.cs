using System;

namespace ET.Server
{
    public abstract class AEventPlus<E, A>: AShareEvent<E, A> where E: Entity where A: struct
    {
        protected abstract ETTask Run(E entity, A a);

        public override async ETTask HandleAsync(E entity, A a)
        {
            try
            {
                await Run(entity, a);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        public override void HandleSync(E entity, A a)
        {
            try
            {
                Run(entity, a).Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}