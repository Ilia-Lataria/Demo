// Net
using System;
using System.Collections.Generic;

// Unity
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 3D Party
using Zenject;

// Project dependencies
using Infable.DarkWaves.Core.Services;
using Infable.DarkWaves.Core.Services.SceneLoader;
using Infable.DarkWaves.Core.Services.DebugLoggers;


namespace Infable.DarkWaves.Core.Base.Models
{
    public abstract class BaseModel : IDisposable
    {
        protected event Action OnModelReady;
        protected readonly SignalBus _signalBus;
        protected readonly FadeHelper _fadeHelper;
        protected readonly CoroutineHelper _coroutineHelper;
        protected readonly SceneLoaderService _sceneLoaderService;
        protected readonly IDebugLogger _logger;

        protected List<IDisposable> _disposables;
        protected List<AsyncOperationHandle> _addressablesAsyncOperationHandlersDisposeTargets;
        protected BaseModel(
            SignalBus signalBus,
            IDebugLogger logger,
            FadeHelper fadeHelper,
            SceneLoaderService sceneLoaderService)
        {
            _logger = logger;
            _signalBus = signalBus;
            _fadeHelper = fadeHelper;
            _sceneLoaderService = sceneLoaderService;
            _disposables = new List<IDisposable>();
            _addressablesAsyncOperationHandlersDisposeTargets = new List<AsyncOperationHandle>();
        }

        public virtual void Dispose()
        {
            for (int i = 0; i < _disposables.Count; i++)
                _disposables[i].Dispose();

            for (int i = 0; i < _addressablesAsyncOperationHandlersDisposeTargets.Count; i++)
                Addressables.Release(_addressablesAsyncOperationHandlersDisposeTargets[i]);

            _logger.Log($"{GetType()} disposed");
        }
    }
}


