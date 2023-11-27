// Net
using System;
using System.Collections.Generic;

// Unity
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 3D Party
using Zenject;
using Cysharp.Threading.Tasks;

// Project dependencies
using Infable.DarkWaves.Core.Services.DebugLoggers;

namespace Infable.DarkWaves.Core.Services.Base
{
    public abstract class BaseService : IDisposable
    {
        protected SignalBus _signalBus;
        protected IDebugLogger _logger;
        protected List<IDisposable> _disposables;
        private List<AsyncOperationHandle> _addressablesAsyncOperationHandlersDisposables;

        public BaseService(
            SignalBus signalBus,
            IDebugLogger logger)
        {
            _logger = logger;
            _signalBus = signalBus;
            _disposables = new List<IDisposable>();
            _addressablesAsyncOperationHandlersDisposables = new List<AsyncOperationHandle>();
            Initialize().Forget();
        }

        protected void AddHandleForDisposing(AsyncOperationHandle handle)
        {
            if (_addressablesAsyncOperationHandlersDisposables.Contains(handle))
            {
                _logger.Warning($"Handler already exist in disposables list");
                return;
            }

            _addressablesAsyncOperationHandlersDisposables.Add(handle);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async UniTaskVoid Initialize() { }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public virtual void Dispose()
        {
            for (int i = 0; i < _disposables.Count; i++)
                _disposables[i].Dispose();
            
            for (int i = 0; i < _addressablesAsyncOperationHandlersDisposables.Count; i++)
                Addressables.Release(_addressablesAsyncOperationHandlersDisposables[i]);
            
            _logger.Log($"{GetType()} addressables handlers disposed");
        }
    }
}
