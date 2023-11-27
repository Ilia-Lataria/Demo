// Unity
using UnityEngine;
using UnityEngine.UI;

// 3D Party
using TMPro;
using Zenject;
using Cysharp.Threading.Tasks;

// Project dependencies
using Infable.DarkWaves.Core.Base.Views;
using Infable.DarkWaves.Core.Scenes.Base.Models;


namespace Infable.DarkWaves.Core.Scenes.Base
{
    public abstract class SceneLoaderView : BaseView
    {
        [SerializeField] protected Slider _nextSceneLoadingProgressSlider;
        [SerializeField] protected TextMeshProUGUI _nextSceneLoadingInfoTxt;
        [SerializeField] protected TextMeshProUGUI _nextSceneLoadingProgressTxt;

        [SerializeField] protected Slider _nextSceneDependenciesLoadingprogressSlider;
        [SerializeField] protected TextMeshProUGUI _nextSceneDependenciesLoadingInfoTxt;
        [SerializeField] protected TextMeshProUGUI _nextSceneDependenciesLoadingProgressTxt;

        private string sceneToLoad;

        public SceneLoadingModel LoadingSceneModel { get; private set; }

        [Inject]
        private void Construct(SceneLoadingModel loadingSceneModel)
        {
            _baseModel = LoadingSceneModel = loadingSceneModel;
            LoadingSceneModel = loadingSceneModel;
        }

        protected override async UniTask Initialize()
        {
            _ = _fadeHelper.FadeInAsync(true);

            sceneToLoad = _bridgeData.GetString($"SceneToLoad", "SceneLogin");
            await LoadingSceneModel.InitializeAsync(sceneToLoad);

            _ = _fadeHelper.FadeOutAsync();

            _nextSceneLoadingInfoTxt.text = $"Computing...";
            _nextSceneDependenciesLoadingInfoTxt.text = $"Computing...";
            _nextSceneDependenciesLoadingprogressSlider.value = 0f;
            _nextSceneDependenciesLoadingProgressTxt.text = $"";

            
            LoadingSceneModel.SubscribeOnNextSceneProgressCanBeRead(OnNextSceneProgressCanBeRead);
            LoadingSceneModel.SubscribeOnNextSceneDependenciesProgressCanBeRead(OnNextSceneDependenciesProgressCanBeRead);

            await LoadingSceneModel.InitializeAsync(sceneToLoad);

            _logger.Log($"Initialize");
        }

        private void OnNextSceneProgressCanBeRead()
            => _ = DisplayNextSceneLoadingProgress();
        private void OnNextSceneDependenciesProgressCanBeRead()
            => _ = DisplayNextSceneDependenceisLoadingProgress();

        private async UniTask DisplayNextSceneLoadingProgress()
        {
            while (true)
            {
                SetViewValues(
                    "Scene",
                    _nextSceneLoadingProgressSlider,
                    _nextSceneLoadingProgressTxt,
                    _nextSceneLoadingInfoTxt,
                    LoadingSceneModel.NextSceneLoadingProgress,
                    LoadingSceneModel.NextSceneTotalBytes,
                    LoadingSceneModel.NextSceneDownloadedBytes);

                if (LoadingSceneModel.NextSceneLoadingProgress == 1f)
                {
                    _logger.Log($"Stop loading view");
                    break;
                }
                await UniTask.Yield();
            }
        }

        private async UniTask DisplayNextSceneDependenceisLoadingProgress()
        {
            while (true)
            {
                SetViewValues(
                    "Dependencies",
                    _nextSceneDependenciesLoadingprogressSlider,
                    _nextSceneDependenciesLoadingProgressTxt,
                    _nextSceneDependenciesLoadingInfoTxt,
                   LoadingSceneModel.NextSceneDependenciesLoadingProgress,
                   LoadingSceneModel.NextSceneDependenciesTotalBytes,
                   LoadingSceneModel.NextSceneDependenciesDownloadedBytes);

                if (LoadingSceneModel.NextSceneDependenciesLoadingProgress == 1f)
                {
                    _logger.Log($"Stop loading view");
                    break;
                }
                await UniTask.Yield();
            }
        }

        private void SetViewValues(
            string infoTxtPrefix,
            Slider loadingProgressSlider,
            TextMeshProUGUI loadingProgressTxt,
            TextMeshProUGUI loadingProgressInfoTxt,
            float loadingProgress, int totalBytes, int downloadedBytes)
        {
            bool loaded = loadingProgress >= 1f;

            loadingProgressInfoTxt.text = loaded ? $"{infoTxtPrefix} loaded {sceneToLoad}" : $"{infoTxtPrefix} loading {sceneToLoad}...";
            loadingProgressSlider.value = loaded ? 1 : loadingProgress;
            loadingProgressTxt.text = loaded ? $"Done" :
                $"{downloadedBytes / 1024} / {totalBytes / 1024} KB";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LoadingSceneModel.UnsubscribeOnNextSceneProgressCanBeRead(OnNextSceneProgressCanBeRead);
            LoadingSceneModel.UnsubscribeOnNextSceneDependenciesProgressCanBeRead(OnNextSceneDependenciesProgressCanBeRead);
        }
    }
}

