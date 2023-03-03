using System;

namespace ET
{
	public interface IUnload
	{
	}
	
	public interface IUnloadSystem: ISystemType
	{
		void Run(Entity o);
	}

	[ObjectSystem]
	public abstract class UnloadSystem<T> : IUnloadSystem where T: Entity, IUnload
	{
		void IUnloadSystem.Run(Entity o)
		{
			this.Unload((T)o);
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IUnloadSystem);
		}

		InstanceQueueIndex ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.Unload;
		}

		protected abstract void Unload(T self);
	}
}
