using Luban;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
	/// <summary>
    /// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
    /// </summary>
    public class ConfigComponent: Singleton<ConfigComponent>
    {
        public struct GetAllConfigBytes
        {
        }
        
        public struct GetOneConfigBytes
        {
            public string ConfigName;
        }
		
        private readonly Dictionary<Type, IConfig> allConfig = new Dictionary<Type, IConfig>();

		public object LoadOneConfig(Type configType)
		{
			this.allConfig.TryGetValue(configType, out IConfig oneConfig);
			ByteBuf oneConfigBytes = EventSystem.Instance.Invoke<GetOneConfigBytes, ByteBuf>(new GetOneConfigBytes() {ConfigName = configType.Name});
			
			object category = Activator.CreateInstance(configType, oneConfigBytes);
			ISingleton singleton = category as ISingleton;
			singleton.Register();
			
			this.allConfig[configType] = singleton as IConfig;
			return category;
		}
		
		// public void Load()
		// {
		// 	this.allConfig.Clear();
		// 	var configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, ByteBuf>>(new GetAllConfigBytes());
		//
		// 	foreach (Type type in configBytes.Keys)
		// 	{
		// 		var oneConfigBytes = configBytes[type];
		// 		this.LoadOneInThread(type, oneConfigBytes);
		// 	}
		// }
		
		public async ETTask LoadAsync()
		{
			this.allConfig.Clear();
			var configBytes = await EventSystem.Instance.Invoke<GetAllConfigBytes, ETTask<Dictionary<Type, ByteBuf>>>(new GetAllConfigBytes());

#if UNITY_WEBGL 
			//注意，此处时为了兼容WebGL中无法使用多线程的情况
			foreach (Type type in configBytes.Keys)
			{
				var oneConfigBytes = configBytes[type];
				LoadOneInThread(type, oneConfigBytes);
			}
#else
			using ListComponent<Task> listTasks = ListComponent<Task>.Create();
			
			foreach (Type type in configBytes.Keys)
			{
				var oneConfigBytes = configBytes[type];
				Task task = Task.Run(() => LoadOneInThread(type, oneConfigBytes));
				listTasks.Add(task);
			}

			await Task.WhenAll(listTasks.ToArray());
#endif
			ResolveRef();
		}
		
		private void LoadOneInThread(Type configType, ByteBuf oneConfigBytes)
		{
			// object category = SerializeHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
			object category = Activator.CreateInstance(configType, oneConfigBytes);
			
			lock (this)
			{
				ISingleton singleton = category as ISingleton;
				singleton.Register();
				this.allConfig[configType] = singleton as IConfig;
			}
		}
		
		private void ResolveRef()
		{
			foreach (var targetConfig in this.allConfig.Values)
			{
				targetConfig.ResolveRef();
			}

			foreach (var targetConfig in this.allConfig.Values)
			{
				Initialized(targetConfig);
			}
		}
		
		private void Initialized(IConfig configCategory)
		{
			var iConfigSystems = EventSystem.Instance.typeSystems.GetSystems(configCategory.GetType(), typeof(IConfigSystem));
			if (iConfigSystems == null)
			{
				return;
			}

			foreach (IConfigSystem aConfigSystem in iConfigSystems)
			{
				if (aConfigSystem == null)
				{
					continue;
				}

				try
				{
					aConfigSystem.Initialized(configCategory);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

#if UNITY_EDITOR
		//编辑器上要是想使用配置表，那么在读取配置表之前先调用这个方法，确保配置表单例被正常加载
		public static void TryLoadForEditor()
		{
			if (Instance == null)
			{
				//编辑器下，如果触发了一次重新编译，那么其单例实例就会被销毁，所以需要重新创建
				if (new ConfigComponent() is ISingleton inst)
				{
					inst.Register();
				}

				Instance.LoadForEditor();
			}
		}

		private void LoadForEditor()
		{
			this.allConfig.Clear();

			//编辑器下，如果触发了一次重新编译，那么其单例实例就会被销毁，所以需要重新创建
			if (EventSystem.Instance == null)
			{
				if (new EventSystem() is ISingleton inst)
				{
					inst.Register();

					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
					EventSystem.Instance.Add(types);
				}
			}

			Dictionary<Type, ByteBuf> configBytes = new();
			HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof(ConfigAttribute));

			foreach (Type configType in configTypes)
			{
				var configFilePath = $"Assets/Art/Config/{configType.Name}.bytes";
				configBytes[configType] = new ByteBuf(File.ReadAllBytes(configFilePath));
			}

			foreach (Type type in configBytes.Keys)
			{
				var oneConfigBytes = configBytes[type];
				this.LoadOneInThread(type, oneConfigBytes);
			}
		}
#endif
	}
}