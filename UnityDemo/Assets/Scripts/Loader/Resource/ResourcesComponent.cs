using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UniFramework.Event;
using YooAsset;

namespace ET
{
    public class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        private ResourcePackage _Package;
        
        /// <summary>
        /// 资源文件查询服务类
        /// </summary>
        public class GameQueryServices : IBuildinQueryServices
        {
            /// <summary>
            /// 查询内置文件的时候，是否比对文件哈希值
            /// </summary>
            public static bool CompareFileCRC = false;

            public bool Query(string packageName, string fileName, string fileCRC)
            {
                // 注意：fileName包含文件格式
                return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
            }
        }
        
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
        /// 资源文件解密流
        /// </summary>
        public class BundleStream : FileStream
        {
            public const byte KEY = 64;

            public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
            {
            }
            public BundleStream(string path, FileMode mode) : base(path, mode)
            {
            }

            public override int Read(byte[] array, int offset, int count)
            {
                var index = base.Read(array, offset, count);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] ^= KEY;
                }
                return index;
            } 
        }
        
        /// <summary>
        /// 资源文件流加载解密类
        /// </summary>
        private class FileStreamDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                BundleStream bundleStream =
                    new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo,
                out Stream managedStream)
            {
                BundleStream bundleStream =
                    new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            private static uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }

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
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode ePlayMode = globalConfig.EPlayMode;
            
            _Package = YooAssets.TryGetPackage(packageName);
            // 创建资源包裹类
            if(_Package == null)
            {
                _Package = YooAssets.CreatePackage(packageName);
            }
            
            if (isDefault)
            {
                // 设置默认的资源包
                YooAssets.SetDefaultPackage(_Package);
            }
            
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
                {
                    string defaultHostServer = GetHostServerURL();
                    string fallbackHostServer = GetHostServerURL();
                    var createParameters = new WebPlayModeParameters();
                    createParameters.DecryptionServices = new FileStreamDecryption();
                    createParameters.BuildinQueryServices = new GameQueryServices();
                    createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    await _Package.InitializeAsync(createParameters).Task;
                    break;
                }
                default:
                    Log.Error($"没有实现这个模式 {ePlayMode}");
                    throw new ArgumentOutOfRangeException();
            }
        }
        
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

        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssets();
        }

        /// <summary>
        /// 主要用来加载dll config aotdll
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public T LoadAsset<T>(string location) where T : UnityEngine.Object
        {
            AssetHandle handle = YooAssets.LoadAssetSync<T>(location);
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }
        
        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            AssetHandle handle = YooAssets.LoadAssetAsync<T>(location);
            await handle.Task;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            AllAssetsHandle allAssetsOperationHandle = YooAssets.LoadAllAssetsAsync<T>(location);
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
        
        public async ETTask ApplyPatchAsync(MonoBehaviour behaviour)
        {
            // 游戏管理器
            GameManager.Instance.Behaviour = behaviour;
            
            // 如果初始化失败弹出提示界面
            if (_Package.InitializeStatus != EOperationStatus.Succeed)
            {
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }

            //2.获取资源版本 FsmRequestPackageVersion
            var operation = _Package.UpdatePackageVersionAsync();
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Log.Error(operation.Error);
            }
            string packageVersion = operation.PackageVersion;

            //3.更新补丁清单 FsmUpdatePackageManifest
            var operation2 = _Package.UpdatePackageManifestAsync(packageVersion);
            await operation2.Task;

            if (operation2.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError(operation2.Error);
                return;
            }
            
            await ETTask.CompletedTask;
        }
    }
}