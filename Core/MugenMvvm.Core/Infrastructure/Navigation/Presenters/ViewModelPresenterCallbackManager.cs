using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenterCallbackManager : HasListenersBase<IViewModelPresenterCallbackManagerListener>, IViewModelPresenterCallbackManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenterCallbackManager()
        {
        }

        #endregion

        #region Properties

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

        public void OnNavigated(IViewModelPresenter presenter, INavigationContext context)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(context, nameof(context));
            OnNavigatedInternal(presenter, context);
        }

        public void OnNavigationFailed(IViewModelPresenter presenter, INavigationContext context, Exception exception)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(presenter, context, exception);
        }

        public void OnNavigationCanceled(IViewModelPresenter presenter, INavigationContext context)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(context, nameof(context));
            OnNavigationCanceledInternal(presenter, context);
        }

        public void OnNavigatingCanceled(IViewModelPresenter presenter, INavigationContext context)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(context, nameof(context));
            OnNavigatingCanceledInternal(presenter, context);
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
            var callbacks = viewModel.Metadata.GetOrAdd(key, (object)null, (object)null, (context, o, arg3) => new List<INavigationCallbackInternal>());
            lock (callback)
            {
                callbacks.Add(callback);
            }

            return callback;
        }

        protected virtual void OnNavigatedInternal(IViewModelPresenter presenter, INavigationContext context)
        {
            if (context.NavigationMode.IsClose)
            {
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, context, Default.TrueObject, null, false);
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.TrueObject, null, false);
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, context, Default.TrueObject, null, false);
            }
            else
                InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, context, Default.TrueObject, null, false);
        }

        protected virtual void OnNavigationFailedInternal(IViewModelPresenter presenter, INavigationContext context, Exception exception)
        {
            OnNavigationFailed(context, exception, false);
        }

        protected virtual void OnNavigationCanceledInternal(IViewModelPresenter presenter, INavigationContext context)
        {
            OnNavigationFailed(context, null, true);
        }

        protected virtual void OnNavigatingCanceledInternal(IViewModelPresenter presenter, INavigationContext context)
        {
            if (context.NavigationMode.IsClose)
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, null, false);
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
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnCallbackAdded(this, viewModel, callback, presenterResult);
        }

        protected virtual void OnCallbackExecuted(IViewModelBase viewModel, INavigationCallback callback, INavigationContext? navigationContext)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnCallbackExecuted(this, viewModel, callback, navigationContext);
        }

        protected void InvokeCallbacks(IViewModelBase? viewModel, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, INavigationContext navigationContext, object result,
            Exception exception, bool canceled)
        {
            if (viewModel == null) //todo trace
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

        private void OnNavigationFailed(INavigationContext context, Exception? e, bool canceled)
        {
            if (context.NavigationMode.IsClose)
            {
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, e, canceled);
                if (!canceled)
                {
                    InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, context, Default.FalseObject, e, false);
                    InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, context, Default.FalseObject, e, false);
                }
            }
            else
            {
                InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, context, Default.FalseObject, e, canceled);
                if (context.NavigationMode.IsNew || !canceled)
                {
                    InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, e, canceled);
                    InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.CloseCallbacks, context, Default.FalseObject, e, canceled);
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