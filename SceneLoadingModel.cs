// Net
using System;
using System.Diagnostics;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

// 3D Party
using Zenject;
using Cysharp.Threading.Tasks;

// Project dependencies
using Infable.DarkWaves.Core.Services;
using Infable.DarkWaves.Core.Base.Models;
using Infable.DarkWaves.Core.Base.Scenes.View;
using Infable.DarkWaves.Core.Services.SceneLoader;
using Infable.DarkWaves.Core.Services.DebugLoggers;
namespace Infable.DarkWaves.Core.Scenes.Base.Models
{
    public abstract class SceneLoadingModel : BaseModel
    {
        private string _sceneToLoadAddresss;
        private event Action OnNextSceneLoaded;
        private event Action OnNextSceneProgressCanBeRead;
        private event Action OnNextSceneDependenciesProgressCanBeRead;
        private AsyncOperationHandle<SceneInstance> _nextSceneAssetLoadingHandle;

        public float NextSceneLoadingProgress { get; set; }
        public int NextSceneTotalBytes { get; private set; }
        public int NextSceneDownloadedBytes { get; private set; }

        public float NextSceneDependenciesLoadingProgress { get; set; }
        public int NextSceneDependenciesTotalBytes { get; private set; }
        public int NextSceneDependenciesDownloadedBytes { get; private set; }

        private readonly Stopwatch _overallProcessTimer;
        public SceneModel NextSceneModel { get; private set; }

        protected SceneLoadingModel(
            SignalBus signalBus,
            IDebugLogger logger,
            FadeHelper fadeHelper,
            SceneLoaderService sceneLoaderService) 
            : base(
                  signalBus,
                  logger, 
                  fadeHelper, 
                  sceneLoaderService) 
        {
            _overallProcessTimer = new Stopwatch();
        }

        public void SubscribeOnNextSceneProgressCanBeRead(Action action)
            => OnNextSceneProgressCanBeRead += action;
        public void UnsubscribeOnNextSceneProgressCanBeRead(Action action) 
            => OnNextSceneProgressCanBeRead -= action;

        public void SubscribeOnNextSceneDependenciesProgressCanBeRead(Action action)
           => OnNextSceneDependenciesProgressCanBeRead += action;
        public void UnsubscribeOnNextSceneDependenciesProgressCanBeRead(Action action)
            => OnNextSceneDependenciesProgressCanBeRead -= action;

        public virtual async UniTask InitializeAsync(string sceneToLoadAddress)
        {
            _logger.Start($"Initialize {sceneToLoadAddress}");
           
            _sceneToLoadAddresss = sceneToLoadAddress;

            if (string.IsNullOrEmpty(_sceneToLoadAddresss))
            {
                _logger.Error($"Error! sceneToLoadAddress IsNullOrEmpty");
                return;
            }

            NextSceneTotalBytes = (int)(await Addressables.GetDownloadSizeAsync(_sceneToLoadAddresss));
            _logger.Success($"Initialize {sceneToLoadAddress} {_overallProcessTimer.ElapsedMilliseconds}ms");
        }

        public async UniTask StartLoading()
        {
            _overallProcessTimer.Start();
            OnNextSceneProgressCanBeRead?.Invoke();

            await _fadeHelper.FadeOutAsync();

            float loadTime = await LoadNextSceneAndReadProgress();

            if (loadTime < Constants.MINIMUM_LOADING_TIME)
            {
                float differenceInSeconds = (Constants.MINIMUM_LOADING_TIME - loadTime) / 1000f;
                _logger.Log($"Loading was too fast lets wait {differenceInSeconds}");

                await UniTask.WaitForSeconds(differenceInSeconds);
            }

            await _fadeHelper.FadeInAsync();
            await ActivateNextScene();

            _overallProcessTimer.Stop();   
        }


        public virtual async UniTask<float> LoadNextSceneAndReadProgress()
        {
            _logger.Start($"LoadNextSceneAndReadProgress");
            Stopwatch processTimer = Stopwatch.StartNew();
            processTimer.Start();

            _nextSceneAssetLoadingHandle = Addressables.LoadSceneAsync(_sceneToLoadAddresss, LoadSceneMode.Additive, false);
            
            while (!_nextSceneAssetLoadingHandle.IsDone)
            {
                NextSceneDownloadedBytes = (int)_nextSceneAssetLoadingHandle.GetDownloadStatus().DownloadedBytes;
                NextSceneLoadingProgress = _nextSceneAssetLoadingHandle.GetDownloadStatus().Percent;
                await UniTask.NextFrame();
            }

            processTimer.Stop();

            NextSceneDownloadedBytes = NextSceneTotalBytes;
            OnNextSceneLoaded?.Invoke();
            _logger.Success($"LoadNextSceneAndReadProgress [{processTimer.ElapsedMilliseconds}ms]");
            return processTimer.ElapsedMilliseconds;
        }

        public async UniTask ActivateNextScene()
        {
            _logger.Start($"ActivateNextScene");
            Stopwatch processTimer = Stopwatch.StartNew();
            processTimer.Start();
        
            await _nextSceneAssetLoadingHandle.Result.ActivateAsync();

            SceneView nextSceneView = GameObject.FindObjectOfType<SceneView>();
            NextSceneModel = nextSceneView.SceneModel;
            NextSceneDependenciesReadProgress();
            OnNextSceneDependenciesProgressCanBeRead?.Invoke();

            processTimer.Stop();
            _logger.Success($"ActivateNextScene [{processTimer.ElapsedMilliseconds}ms]");
        }

        public virtual async UniTask NextSceneDependenciesReadProgress()
        {
            while (NextSceneModel.SceneDependenciesLoadingProgress < 1f)
            {
                await UniTask.NextFrame();
                NextSceneDependenciesDownloadedBytes = NextSceneModel.SceneDependenciesDownloadedBytes;
                NextSceneDependenciesLoadingProgress = NextSceneModel.SceneDependenciesLoadingProgress; 
            }
        }
    }
}

