using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
        private INavigationContext? _navigationContext;
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

        public ItemOrIReadOnlyList<INavigationCallbackListener> GetCallbacks()
        {
            lock (this)
            {
                if (_callbacks is List<INavigationCallbackListener> callbacks)
                    return callbacks.ToItemOrList(false);

                return ItemOrIReadOnlyList.FromItem((INavigationCallbackListener) _callbacks!);
            }
        }

        public bool TryGetResult([NotNullWhen(true)] out INavigationContext? navigationContext)
        {
            navigationContext = _state == SuccessState ? _navigationContext : null;
            return navigationContext != null;
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
                        var editor = GetCallbacksEditorRaw();
                        editor.Add(callback);
                        _callbacks = editor.GetRawValue();
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
                var list = GetCallbacksEditorRaw();
                list.Remove(callback);
                _callbacks = list.GetRawValue();
            }
        }

        #endregion

        #region Methods

        public bool TrySetResult(INavigationContext navigationContext) => SetResult(SuccessState, navigationContext, null, default, false);

        public void SetResult(INavigationContext navigationContext) => SetResult(SuccessState, navigationContext, null, default, true);

        public bool TrySetException(INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            return SetResult(ErrorState, navigationContext, exception, default, false);
        }

        public void SetException(INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(exception, nameof(exception));
            SetResult(ErrorState, navigationContext, exception, default, true);
        }

        public bool TrySetCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => SetResult(CanceledState, navigationContext, null, cancellationToken, false);

        public void SetCanceled(INavigationContext navigationContext, CancellationToken cancellationToken) => SetResult(CanceledState, navigationContext, null, cancellationToken, true);

        private bool SetResult(int state, INavigationContext navigationContext, Exception? exception, CancellationToken cancellationToken, bool throwOnError)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var completed = false;
            ItemOrIReadOnlyList<INavigationCallbackListener> callbacks = default;
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
                        _navigationContext = navigationContext;
                        completed = true;
                    }
                }
            }

            if (completed)
            {
                foreach (var listener in callbacks)
                    InvokeCallback(listener);
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
                    callback.OnCompleted(_navigationContext!);
                    break;
                case ErrorState:
                    callback.OnError(_navigationContext!, _exception!);
                    break;
                case CanceledState:
                    callback.OnCanceled(_navigationContext!, _cancellationToken);
                    break;
            }
        }

        private ItemOrIReadOnlyList<INavigationCallbackListener> GetCallbacksRaw() => ItemOrIReadOnlyList.FromRawValue<INavigationCallbackListener>(_callbacks);

        private ItemOrListEditor<INavigationCallbackListener> GetCallbacksEditorRaw() => ItemOrListEditor<INavigationCallbackListener>.FromRawValue(_callbacks);

        #endregion
    }
}