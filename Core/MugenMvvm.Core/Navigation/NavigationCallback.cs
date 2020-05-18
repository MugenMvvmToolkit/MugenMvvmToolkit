using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationCallback : INavigationCallback
    {
        #region Fields

        private object? _callbacks;
        private CancellationToken _cancellationToken;
        private Exception? _exception;
        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int SuccessState = 1;
        private const int ErrorState = 2;
        private const int CanceledState = 3;

        #endregion

        #region Constructors

        public NavigationCallback(NavigationCallbackType callbackType, string navigationId, NavigationType navigationType)
        {
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNullOrEmpty(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            CallbackType = callbackType;
            NavigationId = navigationId;
            NavigationType = navigationType;
        }

        #endregion

        #region Properties

        public bool IsCompleted => _state != 0;

        public NavigationCallbackType CallbackType { get; }

        public string NavigationId { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<INavigationCallbackListener, IReadOnlyList<INavigationCallbackListener>> GetCallbacks()
        {
            lock (this)
            {
                return ItemOrList<INavigationCallbackListener, IReadOnlyList<INavigationCallbackListener>>.FromRawValue(_callbacks);
            }
        }

        public void AddCallback(INavigationCallbackListener callback)
        {
            Should.NotBeNull(callback, nameof(callback));
            if (!IsCompleted)
            {
                lock (this)
                {
                    if (!IsCompleted)
                    {
                        var list = GetCallbacksRaw();
                        list.Add(callback);
                        _callbacks = list.GetRawValue();
                        return;
                    }
                }
            }

            InvokeCallback(callback);
        }

        public void RemoveCallback(INavigationCallbackListener callback)
        {
            lock (this)
            {
                var list = GetCallbacksRaw();
                list.Remove(callback);
                _callbacks = list.GetRawValue();
            }
        }

        #endregion

        #region Methods

        public bool TrySetResult(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return SetResult(SuccessState, null, metadata, default, false);
        }

        public void SetResult(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            SetResult(SuccessState, null, metadata, default, true);
        }

        public bool TrySetException(Exception exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(exception, nameof(exception));
            return SetResult(ErrorState, exception, metadata, default, false);
        }

        public void SetException(Exception exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(exception, nameof(exception));
            SetResult(ErrorState, exception, metadata, default, true);
        }

        public bool TrySetCanceled(IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return SetResult(CanceledState, null, metadata, cancellationToken, false);
        }

        public void SetCanceled(IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            SetResult(CanceledState, null, metadata, cancellationToken, true);
        }

        private bool SetResult(int state, Exception? exception, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken, bool throwOnError)
        {
            var completed = false;
            ItemOrList<INavigationCallbackListener, List<INavigationCallbackListener>> callbacks = default;
            if (!IsCompleted)
            {
                lock (this)
                {
                    if (!IsCompleted)
                    {
                        callbacks = GetCallbacksRaw();
                        _callbacks = null;
                        _cancellationToken = cancellationToken;
                        _state = state;
                        _exception = exception;
                        _metadata = metadata;
                        completed = true;
                    }
                }
            }

            if (completed)
            {
                for (var i = 0; i < callbacks.Count(); i++)
                    InvokeCallback(callbacks.Get(i));
                return true;
            }

            if (throwOnError)
                ExceptionManager.ThrowObjectInitialized(this);
            return false;
        }

        private void InvokeCallback(INavigationCallbackListener callback)
        {
            switch (_state)
            {
                case SuccessState:
                    callback.OnCompleted(_metadata!);
                    break;
                case ErrorState:
                    callback.OnError(_exception!, _metadata);
                    break;
                case CanceledState:
                    callback.OnCanceled(_metadata, _cancellationToken);
                    break;
            }
        }

        private ItemOrList<INavigationCallbackListener, List<INavigationCallbackListener>> GetCallbacksRaw()
        {
            return ItemOrList<INavigationCallbackListener, List<INavigationCallbackListener>>.FromRawValue(_callbacks);
        }

        #endregion
    }
}