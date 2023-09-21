// 该版本实现并不会执行联网下载，参考Yooasset插件里的Sample另外实现了一版
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace ET
{
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    public class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    
    /// <summary>
    /// 资源管理组件
    /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
    /// </summary>
    public partial class ResourcesComponentOld: Singleton<ResourcesComponentOld>, ISingletonAwake
    {
        private ResourcePackage _Package;
        
        public void Awake()
        {
            YooAssets.Initialize();
        }

        public override void Dispose()
        {
            YooAssets.Destroy();
        }
        
        public async ETTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            _Package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                YooAssets.SetDefaultPackage(_Package);
            }

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode ePlayMode = globalConfig.EPlayMode;

            #if !UNITY_EDITOR
            if (ePlayMode == EPlayMode.EditorSimulateMode)
            {
                ePlayMode = EPlayMode.OfflinePlayMode;
                Log.Error($"当前处于非编辑器模式 但是选择的是编辑器模式加载资源 强制修改为 OfflinePlayMode");
            }
            #endif
            
            // 编辑器下的模拟模式
            switch (ePlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                {
                    EditorSimulateModeParameters createParameters = new();
                    createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("ScriptableBuildPipeline", packageName);
                    await _Package.InitializeAsync(createParameters).Task;
                    break;
                }
                case EPlayMode.OfflinePlayMode:
                {
                    OfflinePlayModeParameters createParameters = new();
                    await _Package.InitializeAsync(createParameters).Task;
                    break;
                }
                case EPlayMode.HostPlayMode:
                {
                    string defaultHostServer = GetHostServerURL();
                    string fallbackHostServer = GetHostServerURL();
                    HostPlayModeParameters createParameters = new();
                    createParameters.BuildinQueryServices = new GameQueryServices();
                    createParameters.RemoteServices       = new RemoteServices(defaultHostServer, fallbackHostServer);
                    await _Package.InitializeAsync(createParameters).Task;
                    break;
                }
                case EPlayMode.WebPlayMode:
                default:
                    Log.Error($"没有实现这个模式 {ePlayMode}");
                    throw new ArgumentOutOfRangeException();
            }

            return;

            string GetHostServerURL()
            {
                //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
                string hostServerIP = "http://127.0.0.1";
                string appVersion = "v1.0";

#if UNITY_EDITOR
                if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                    return $"{hostServerIP}/CDN/Android/{appVersion}";
                else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
                else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                    return $"{hostServerIP}/CDN/WebGL/{appVersion}";
                else
                    return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
		        if (Application.platform == RuntimePlatform.Android)
		        	return $"{hostServerIP}/CDN/Android/{appVersion}";
		        else if (Application.platform == RuntimePlatform.IPhonePlayer)
		        	return $"{hostServerIP}/CDN/IPhone/{appVersion}";
		        else if (Application.platform == RuntimePlatform.WebGLPlayer)
		        	return $"{hostServerIP}/CDN/WebGL/{appVersion}";
		        else
		        	return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
            }
        }
        
        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssets();
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为各种原因无法使用ResourcesLoaderComponent时。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            var allAssetsOperationHandle = YooAssets.LoadAllAssetsAsync<T>(location);
            await allAssetsOperationHandle.Task;
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            allAssetsOperationHandle.Release();
            return dictionary;
        }
        
        /// <summary>
        /// 主要用来加载dll config aotdll，因为各种原因无法使用ResourcesLoaderComponent时。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetAsync<T>(location);
            await handle.Task;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }
        
        /// <summary>
        /// 主要用来加载dll config aotdll，因为各种原因无法使用ResourcesLoaderComponent时。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public T LoadAsset<T>(string location) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetAsync<T>(location);
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }
    }
}