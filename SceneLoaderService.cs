using System;
using System.Diagnostics;

using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

using Zenject;
using Cysharp.Threading.Tasks;

using Infable.DarkWaves.Core.Services.DebugLoggers;
using Infable.DarkWaves.Core.Services.Base;

namespace Infable.DarkWaves.Core.Services.SceneLoader
{
    public class SceneLoaderService : BaseService
    {
        private readonly BridgeData _bridgeData;
        
        private readonly FadeHelper _fadeHelper;
        private readonly int _nextSceneLoadingDelayTime = 3000; //ms
        private readonly int _minimumTimeForLoadingNextScene = 3000; //ms

        private string _sceneToLoadAddresss;
        private AsyncOperationHandle<SceneInstance> _loadingSceneHandler;

        public SceneLoaderService
        (
            SignalBus signalBus,
            IDebugLogger logger,
            BridgeData bridgeData,
            FadeHelper fadeHelper)
            : base(signalBus, logger)
        {
            _logger = logger;
            _bridgeData = bridgeData;

            _fadeHelper = fadeHelper;
        }

        public async UniTask StartLoadingNextScene(string sceneToLoad, bool isVideoLoader = false)
        {
            _logger.Log($"Load scene by address {sceneToLoad}");
            _sceneToLoadAddresss = sceneToLoad;

            if (string.IsNullOrEmpty(_sceneToLoadAddresss))
            {
                _logger.Error("Should set SceneAddress to load");
                return;
            }

            try
            {
                _bridgeData.SetString("SceneToLoad", _sceneToLoadAddresss);
                await _fadeHelper.FadeInAsync();
                _ = LoadNextSceneAndActivate();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }

        private async UniTask LoadNextSceneAndActivate(bool isVideoLoader = false)
        {
            _logger.Start($"Load and activate loading scene");
            
            Stopwatch processTimer = Stopwatch.StartNew();
            processTimer.Start();
            
            string sceneLoadingAddress = "SceneLoading";
            sceneLoadingAddress = isVideoLoader ? $"{sceneLoadingAddress}Video" : sceneLoadingAddress;

            _loadingSceneHandler = Addressables.LoadSceneAsync(sceneLoadingAddress, LoadSceneMode.Single);
            AddHandleForDisposing(_loadingSceneHandler);

            await _loadingSceneHandler;
            processTimer.Stop();
            _logger.Success($"Load and activate loading scene [{processTimer.ElapsedMilliseconds}ms]");
        }
    }
}

