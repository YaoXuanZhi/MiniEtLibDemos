using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
	public class CodeLoader: Singleton<CodeLoader>
	{
		private Assembly model;
		private Dictionary<string, TextAsset> dlls;
		private Dictionary<string, TextAsset> aotDlls;
		
		public async ETTask DownloadAsync()
		{
			if (!Define.IsEditor)
			{
				this.dlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"Model.dll");
				this.aotDlls = await ResourcesComponent.Instance.LoadAllAssetsAsync<TextAsset>($"mscorlib.dll");
			}
		}

		public void Start()
		{
			if (Define.EnableCodes)
			{
				GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
				}
				
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
				EventSystem.Instance.Add(types);
				foreach (Assembly ass in assemblies)
				{
					string name = ass.GetName().Name;
					if (name == "Unity.Model.Codes")
					{
						this.model = ass;
					}
				}
			}
			else
			{
				byte[] assBytes;
				byte[] pdbBytes;
				if (!Define.IsEditor)
				{
					assBytes = this.dlls["Model.dll"].bytes;
					pdbBytes = this.dlls["Model.pdb"].bytes;

					if (Define.EnableIL2CPP)
					{
						foreach (var kv in this.aotDlls)
						{
							TextAsset textAsset = kv.Value;
							RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
						}
					}
				}
				else
				{
					assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
				}
			
				this.model = Assembly.Load(assBytes, pdbBytes);
				this.LoadHotfix();
			}
			
			IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
			start.Run();
		}

		// 热重载调用该方法
		public void LoadHotfix()
		{
			byte[] assBytes;
			byte[] pdbBytes;
			if (!Define.IsEditor)
			{
				assBytes = this.dlls["Hotfix.dll"].bytes;
				pdbBytes = this.dlls["Hotfix.pdb"].bytes;
			}
			else
			{
				// 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
				string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
				if (logicFiles.Length != 1)
				{
					throw new Exception("Logic dll count != 1");
				}
				string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
				assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
				pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));
			}

			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
			
			Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Init).Assembly, this.model, hotfixAssembly);
			
			EventSystem.Instance.Add(types);
		}
	}
}