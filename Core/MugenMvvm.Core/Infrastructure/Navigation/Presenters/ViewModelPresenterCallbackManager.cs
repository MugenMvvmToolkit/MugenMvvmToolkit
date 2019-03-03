using System;
using System.Linq;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenterCallbackManager : IViewModelPresenterCallbackManager
    {
        #region Fields

        private IComponentCollection<IViewModelPresenterCallbackManagerListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenterCallbackManager(IComponentCollection<IViewModelPresenterCallbackManagerListener>? listeners = null)
        {
            _listeners = listeners;
        }

        #endregion

        #region Properties

        public IComponentCollection<IViewModelPresenterCallbackManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IViewModelPresenterCallbackManagerListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public bool IsSerializable { get; set; }

        #endregion

        #region Implementation of interfaces

        public INavigationCallback AddCallback(IViewModelPresenter presenter, IViewModelBase viewModel, NavigationCallbackType callbackType,
            IChildViewModelPresenterResult presenterResult, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(presenterResult, nameof(presenterResult));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(presenterResult, nameof(presenterResult));
            Should.NotBeNull(metadata, nameof(metadata));
            var callback = AddCallbackInternal(presenter, viewModel, callbackType, presenterResult);
            OnCallbackAdded(viewModel, callback, presenterResult);
            return callback;
        }

        public void OnNavigated(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigatedInternal(presenter, navigationContext);
        }

        public void OnNavigationFailed(IViewModelPresenter presenter, INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(presenter, navigationContext, exception);
        }

        public void OnNavigationCanceled(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigationCanceledInternal(presenter, navigationContext);
        }

        public void OnNavigatingCanceled(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigatingCanceledInternal(presenter, navigationContext);
        }

        public IReadOnlyList<INavigationCallback> GetCallbacks(IViewModelPresenter presenter, INavigationEntry navigationEntry, NavigationCallbackType? callbackType,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetCallbacksInternal(presenter, navigationEntry, callbackType, metadata);
        }

        #endregion

        #region Methods

        protected virtual INavigationCallback AddCallbackInternal(IViewModelPresenter presenter, IViewModelBase viewModel, NavigationCallbackType callbackType,
            IChildViewModelPresenterResult presenterResult)
        {
            var serializable = IsSerializable && callbackType == NavigationCallbackType.Close && presenterResult.Metadata.Get(NavigationInternalMetadata.IsRestorableCallback);
            var callback = new NavigationCallback(callbackType, presenterResult.NavigationType, serializable, presenterResult.NavigationProvider.Id);

            var key = GetKeyByCallback(callbackType);
            if (key == null)
                throw ExceptionManager.EnumOutOfRange(nameof(callbackType), callbackType);
            var callbacks = viewModel.Metadata.GetOrAdd(key, (object) null, (object) null, (context, o, arg3) => new List<INavigationCallbackInternal>());
            lock (callback)
            {
                callbacks.Add(callback);
            }

            return callback;
        }

        protected virtual void OnNavigatedInternal(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, navigationContext, Default.TrueObject, null, false);
                InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, navigationContext, Default.TrueObject, null, false);
                InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, navigationContext, Default.TrueObject, null, false);
            }
            else
                InvokeCallbacks(navigationContext.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, navigationContext, Default.TrueObject, null, false);
        }

        protected virtual void OnNavigationFailedInternal(IViewModelPresenter presenter, INavigationContext navigationContext, Exception exception)
        {
            OnNavigationFailed(navigationContext, exception, false);
        }

        protected virtual void OnNavigationCanceledInternal(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            OnNavigationFailed(navigationContext, null, true);
        }

        protected virtual void OnNavigatingCanceledInternal(IViewModelPresenter presenter, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
                InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, navigationContext, Default.FalseObject, null, false);
        }

        protected virtual IReadOnlyList<INavigationCallback> GetCallbacksInternal(IViewModelPresenter presenter, INavigationEntry navigationEntry,
            NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
        {
            List<INavigationCallback>? callbacks = null;
            if (callbackType == null)
            {
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ShowingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ClosingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.CloseCallbacks, ref callbacks);
            }
            else
            {
                var key = GetKeyByCallback(callbackType);
                if (key != null)
                    AddEntryCallbacks(navigationEntry, key, ref callbacks);
            }

            if (callbacks == null)
                return Default.EmptyArray<INavigationCallback>();
            return callbacks;
        }

        protected virtual IMetadataContextKey<IList<INavigationCallbackInternal?>?>? GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return NavigationInternalMetadata.ShowingCallbacks;
            if (callbackType == NavigationCallbackType.Closing)
                return NavigationInternalMetadata.ClosingCallbacks;
            if (callbackType == NavigationCallbackType.Close)
                return NavigationInternalMetadata.CloseCallbacks;
            return null;
        }

        protected virtual void OnCallbackAdded(IViewModelBase viewModel, INavigationCallback callback, IChildViewModelPresenterResult presenterResult)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnCallbackAdded(this, viewModel, callback, presenterResult);
        }

        protected virtual void OnCallbackExecuted(IViewModelBase viewModel, INavigationCallback callback, INavigationContext? navigationContext)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnCallbackExecuted(this, viewModel, callback, navigationContext);
        }

        protected void InvokeCallbacks(IViewModelBase? viewModel, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, INavigationContext navigationContext,
            object result,
            Exception exception, bool canceled)
        {
            if (viewModel == null)
                return;
            var callbacks = viewModel.Metadata.Get(key);
            if (callbacks == null)
                return;
            List<INavigationCallbackInternal> toInvoke = null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationType == navigationContext.NavigationType && callback.NavigationProviderId == navigationContext.NavigationProvider.Id)
                    {
                        if (callback != null)
                        {
                            if (toInvoke == null)
                                toInvoke = new List<INavigationCallbackInternal>();
                            toInvoke.Add(callback);
                        }

                        callbacks.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (toInvoke == null)
                return;
            for (var i = 0; i < toInvoke.Count; i++)
            {
                var callback = toInvoke[i];
                if (exception != null)
                    callback.SetException(exception, navigationContext);
                else if (canceled)
                    callback.SetCanceled(navigationContext);
                else
                    callback.SetResult(result, navigationContext);
                OnCallbackExecuted(viewModel, callback, navigationContext);
            }
        }

        private void OnNavigationFailed(INavigationContext navigationContext, Exception? e, bool canceled)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, navigationContext, Default.FalseObject, e, canceled);
                if (!canceled)
                {
                    InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, navigationContext, Default.FalseObject, e, false);
                    InvokeCallbacks(navigationContext.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, navigationContext, Default.FalseObject, e, false);
                }
            }
            else
            {
                InvokeCallbacks(navigationContext.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, navigationContext, Default.FalseObject, e, canceled);
                if (navigationContext.NavigationMode.IsNew || !canceled)
                {
                    InvokeCallbacks(navigationContext.ViewModelTo, NavigationInternalMetadata.ClosingCallbacks, navigationContext, Default.FalseObject, e, canceled);
                    InvokeCallbacks(navigationContext.ViewModelTo, NavigationInternalMetadata.CloseCallbacks, navigationContext, Default.FalseObject, e, canceled);
                }
            }
        }

        private static void AddEntryCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key,
            ref List<INavigationCallback>? callbacks)
        {
            var list = navigationEntry.ViewModel.Metadata.Get(key);
            if (list == null)
                return;
            if (callbacks == null)
                callbacks = new List<INavigationCallback>();
            lock (list)
            {
                callbacks.AddRange(list.Where(c => c != null));
            }
        }

        #endregion
    }
}