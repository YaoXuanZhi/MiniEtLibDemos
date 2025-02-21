using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UniFramework.Event;
using UnityEngine;
using YooAsset;

namespace ET
{
    public partial class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        private ResourcePackage _package;

        public void Awake()
        {
            // 初始化事件系统
            UniEvent.Initalize();

            // 初始化YooAssets
            YooAssets.Initialize();
        }

        public override void Dispose()
        {
            UniEvent.Destroy();
        }

        public async ETTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode playMode = globalConfig.EPlayMode;
            
            _package = YooAssets.TryGetPackage(packageName);
            // 创建资源包裹类
            if(_package == null)
            {
                _package = YooAssets.CreatePackage(packageName);
            }
            
            if (isDefault)
            {
                // 设置默认的资源包
                YooAssets.SetDefaultPackage(_package);
            }

            // 编辑器下的模拟模式
            // InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                await _package.InitializeAsync(createParameters).Task;
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                await _package.InitializeAsync(createParameters).Task;
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                await _package.InitializeAsync(createParameters).Task;
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
				                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#else
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters(new WebDecryption());
#endif
                await _package.InitializeAsync(createParameters).Task;
            }
            
            // 如果初始化失败弹出提示界面
            if (_package.InitializeStatus != EOperationStatus.Succeed)
            {
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }

            //2.获取资源版本 FsmRequestPackageVersion
            var operation = _package.RequestPackageVersionAsync();
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Log.Error(operation.Error);
            }
            string packageVersion = operation.PackageVersion;

            //3.更新补丁清单 FsmUpdatePackageManifest
            var operation2 = _package.UpdatePackageManifestAsync(packageVersion);
            await operation2.Task;

            if (operation2.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Log.Error(operation2.Error);
            }
        }
        
        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssetsAsync();
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
    }

    public partial class ResourcesComponent
    {
        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
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

        private class WebDecryption : IWebDecryptionServices
        {
            public const byte KEY = 64;

            public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
            {
                byte[] copyData = new byte[fileInfo.FileData.Length];
                Buffer.BlockCopy(fileInfo.FileData, 0, copyData, 0, fileInfo.FileData.Length);

                for (int i = 0; i < copyData.Length; i++)
                {
                    copyData[i] ^= KEY;
                }

                WebDecryptResult decryptResult = new WebDecryptResult();
                decryptResult.Result = AssetBundle.LoadFromMemory(copyData);
                return decryptResult;
            }
        }

        public async ETTask CreatePackageAsync2(string packageName, bool isDefault = false)
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode playMode = globalConfig.EPlayMode;

            // 创建资源包裹类
            _package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                // 设置默认的资源包
                YooAssets.SetDefaultPackage(_package);
            }

            // 编辑器下的模拟模式
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                await _package.InitializeAsync(createParameters).Task;
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                await _package.InitializeAsync(createParameters).Task;
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                await _package.InitializeAsync(createParameters).Task;
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
				                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#else
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters(new WebDecryption());
#endif
                await _package.InitializeAsync(createParameters).Task;
            }

            // 如果初始化失败弹出提示界面
            if (_package.InitializeStatus != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"======> initializationOperation.Error");

                // Debug.LogWarning($"{initializationOperation.Error}");
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }

            //2.获取资源版本 FsmRequestPackageVersion
            var operation = _package.RequestPackageVersionAsync();
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Log.Error(operation.Error);
                return;
            }

            string packageVersion = operation.PackageVersion;

            //3.更新补丁清单 FsmUpdatePackageManifest
            var operation2 = _package.UpdatePackageManifestAsync(packageVersion);
            await operation2.Task;

            if (operation2.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError(operation2.Error);
                return;
            }
        }

        public async ETTask CreatePackageAsync3(string packageName, bool isDefault = false)
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode playMode = globalConfig.EPlayMode;

            // 创建资源包裹类
            _package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                // 设置默认的资源包
                YooAssets.SetDefaultPackage(_package);
            }

            // 编辑器下的模拟模式
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                await _package.InitializeAsync(createParameters).Task;
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                await _package.InitializeAsync(createParameters).Task;
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                await _package.InitializeAsync(createParameters).Task;
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                var createParameters = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
				                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
#else
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters(new WebDecryption());
#endif
                await _package.InitializeAsync(createParameters).Task;
            }

            // 如果初始化失败弹出提示界面
            if (_package.InitializeStatus != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"======> initializationOperation.Error");

                // Debug.LogWarning($"{initializationOperation.Error}");
                PatchEventDefine.InitializeFailed.SendEventMessage();
            }

            //2.获取资源版本 FsmRequestPackageVersion
            var operation = _package.RequestPackageVersionAsync();
            await operation.Task;

            if (operation.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Log.Error(operation.Error);
                return;
            }

            string packageVersion = operation.PackageVersion;

            //3.更新补丁清单 FsmUpdatePackageManifest
            var operation2 = _package.UpdatePackageManifestAsync(packageVersion);
            await operation2.Task;

            if (operation2.Status != EOperationStatus.Succeed)
            {
                //更新失败
                Debug.LogError(operation2.Error);
                return;
            }
            
            //4.下载补丁包 FsmCreateDownloader
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = _package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
                
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("Not found any download files !");
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);

                //注册回调方法
                downloader.DownloadErrorCallback = OnDownloadErrorFunction;
                downloader.DownloadUpdateCallback = OnDownloadProgressUpdateFunction;
                downloader.DownloadFinishCallback = OnDownloadOverFunction;
                downloader.DownloadFileBeginCallback = OnStartDownloadFileFunction;
                downloader.BeginDownload();
                await downloader.Task;
                
                // 检测下载结果
                if (downloader.Status != EOperationStatus.Succeed)
                {
                }
                
                //5.资源文件下载完毕 FsmDownloadPackageOver
                
                //6.清理未使用的缓存文件 FsmClearCacheBundle
                var operation3 = _package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
                await operation3.Task;
                
                //7.开始游戏 FsmStartGame
            }
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
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
        
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sizeBytes"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnStartDownloadFileFunction(DownloadFileData data)
        {
            string fileName = data.FileName;
            long sizeBytes = data.FileSize;
            Debug.Log(string.Format("开始下载：文件名：{0}, 文件大小：{1}", fileName, sizeBytes));
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="isSucceed"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDownloadOverFunction(DownloaderFinishData data)
        {
            bool isSucceed = data.Succeed;
            Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
        }

        /// <summary>
        /// 更新中
        /// </summary>
        private void OnDownloadProgressUpdateFunction(DownloadUpdateData data)
        {
            int totalDownloadCount = data.TotalDownloadCount;
            int currentDownloadCount = data.CurrentDownloadCount;
            long totalDownloadBytes = data.TotalDownloadBytes;
            long currentDownloadBytes = data.CurrentDownloadBytes;
            Debug.Log(string.Format("文件总数：{0}, 已下载文件数：{1}, 下载总大小：{2}, 已下载大小：{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));
        }

        /// <summary>
        /// 下载出错
        /// </summary>
        private void OnDownloadErrorFunction(DownloadErrorData data)
        {
            string fileName = data.FileName;
            string error = data.ErrorInfo;
            Debug.LogError(string.Format("下载出错：文件名：{0}, 错误信息：{1}", fileName, error));
        }
    }
}