using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;
using YooAsset;

namespace ET
{
	public enum PlatformType
	{
		None,
		Android,
		IOS,
		Windows,
		MacOS,
		Linux
	}
	
	public enum BuildType
	{
		Development,
		Release,
	}

	public class BuildEditor : EditorWindow
	{
		private PlatformType activePlatform;
		private PlatformType platformType;
		private bool clearFolder;
		private bool isBuildExe;
		private bool isContainAB;
		private CodeOptimization codeOptimization = CodeOptimization.Debug;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;

		private GlobalConfig globalConfig;
		private static string reloadConfigName;

		[MenuItem("ET/Build Tool")]
		public static void ShowWindow()
		{
			GetWindow<BuildEditor>(DockDefine.Types);
		}

        private void OnEnable()
		{
			globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Resources/GlobalConfig.asset");
			
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
			activePlatform = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
			activePlatform = PlatformType.Linux;
#else
			activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;
        }

        private void OnGUI() 
		{
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
			this.clearFolder = EditorGUILayout.Toggle("clean folder? ", clearFolder);
			this.isBuildExe = EditorGUILayout.Toggle("build exe?", this.isBuildExe);
			this.isContainAB = EditorGUILayout.Toggle("contain assetsbundle?", this.isContainAB);
			this.codeOptimization = (CodeOptimization)EditorGUILayout.EnumPopup("CodeOptimization ", this.codeOptimization);
			EditorGUILayout.LabelField("BuildAssetBundleOptions ");
			this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);
			
			switch (this.codeOptimization)
			{
				case CodeOptimization.None:
				case CodeOptimization.Debug:
					this.buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
					break;
				case CodeOptimization.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);
			
			if (GUILayout.Button("BuildPackage"))
			{
				if (this.platformType == PlatformType.None)
				{
					ShowNotification(new GUIContent("please select platform!"));
					return;
				}
				if (platformType != activePlatform)
				{
					switch (EditorUtility.DisplayDialogComplex("Warning!", $"current platform is {activePlatform}, if change to {platformType}, may be take a long time", "change", "cancel", "no change"))
					{
						case 0:
							activePlatform = platformType;
							break;
						case 1:
							return;
						case 2:
							platformType = activePlatform;
							break;
					}
				}
				BuildHelper.Build(this.platformType, this.buildAssetBundleOptions, this.buildOptions, this.isBuildExe, this.isContainAB, this.clearFolder);
			}
			
			GUILayout.Label("");
			GUILayout.Label("Code Compile：");
			EditorGUI.BeginChangeCheck();
			this.globalConfig.CodeMode = (CodeMode)EditorGUILayout.EnumPopup("CodeMode: ", this.globalConfig.CodeMode);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(this.globalConfig);
				AssetDatabase.SaveAssetIfDirty(this.globalConfig);
				AssetDatabase.Refresh();
			}
			
			EditorGUI.BeginChangeCheck();
			this.globalConfig.EPlayMode = (EPlayMode)EditorGUILayout.EnumPopup("PlayMode: ", this.globalConfig.EPlayMode);
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(this.globalConfig);
				AssetDatabase.SaveAssetIfDirty(this.globalConfig);
				AssetDatabase.Refresh();
			}

			if (GUILayout.Button("BuildModelAndHotfix"))
			{
				if (Define.EnableCodes)
				{
					throw new Exception("now in ENABLE_CODES mode, do not need Build!");
				}
				BuildAssembliesHelper.BuildModel(this.codeOptimization, globalConfig);
				BuildAssembliesHelper.BuildHotfix(this.codeOptimization, globalConfig);

				AfterCompiling();
				
				ShowNotification("Build Model And Hotfix Success!");
			}
			
			if (GUILayout.Button("BuildHotfix&Reload"))
			{
				BuildAssembliesHelper.BuildHotfix(this.codeOptimization, globalConfig);
				if (EditorApplication.isPlaying)
				{
					CodeLoader.Instance.LoadHotfix();
					EventSystem.Instance.Load();
				}
				ShowNotification(new GUIContent("BuildHotfix&Reload Finish"));
			}
			
			if (GUILayout.Button("ExcelExporter"))
			{
				var configDir = "Assets/Bundles/Config";
				if (Directory.Exists(configDir))
				{
					Directory.Delete(configDir, true);
				}
				ToolsEditor.ExcelExporter(this.globalConfig.CodeMode);
				
				// // 设置ab包
				// AssetImporter assetImporter = AssetImporter.GetAtPath(configDir);
				// assetImporter.assetBundleName = "Config.unity3d";
				// AssetDatabase.SaveAssets();
				// AssetDatabase.Refresh();
			}
			
			if (GUILayout.Button("Proto2CS"))
			{
				Proto2CS.Export();
			}
			
	        EditorGUILayout.Space();
			
			// 编辑器支持配置热重载
	        EditorGUILayout.BeginHorizontal();
	        EditorGUILayout.LabelField("ConfigName:");
	        reloadConfigName = EditorGUILayout.TextField(reloadConfigName);
        	EditorGUILayout.EndHorizontal();
	        if (GUILayout.Button("ReloadConfig"))
	        {
		        if (Application.isPlaying)
		        {
			        if (!string.IsNullOrEmpty(reloadConfigName))
			        {
				        reloadConfigName = reloadConfigName.Trim(' ');
				        if (reloadConfigName.Length > 0)
				        {
					        string category = $"{reloadConfigName}Category";
					        Type type = EventSystem.Instance.GetType($"ET.{category}");
					        if (type == null)
					        {
						        UnityEngine.Debug.LogWarning($"reload config but not find {category}");
						        return;
					        }

					        ConfigComponent.Instance.LoadOneConfig(type);
					        UnityEngine.Debug.Log($"reload config {reloadConfigName} finish!");
				        }
			        }
		        }
	        }

			GUILayout.Space(5);
		}
		
		private static void AfterCompiling()
		{
			Directory.CreateDirectory(BuildAssembliesHelper.CodeDir);

			// 设置ab包
			AssetImporter assetImporter = AssetImporter.GetAtPath("Assets/Bundles/Code");
			assetImporter.assetBundleName = "Code.unity3d";
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
            
			Debug.Log("build success!");
		}
		
		public static void ShowNotification(string tips)
		{
			EditorWindow game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
			game?.ShowNotification(new GUIContent($"{tips}"));
		}
	}
}
