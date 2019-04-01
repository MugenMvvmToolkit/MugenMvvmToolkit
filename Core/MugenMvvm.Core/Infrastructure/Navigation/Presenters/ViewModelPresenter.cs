using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Fields

        private IComponentCollection<IViewModelPresenterListener>? _listeners;
        private IComponentCollection<IChildViewModelPresenter>? _presenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewModelPresenterCallbackManager callbackManager,
            IComponentCollection<IChildViewModelPresenter>? presenters = null, IComponentCollection<IViewModelPresenterListener>? listeners = null)
        {
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            CallbackManager = callbackManager;
            _presenters = presenters;
            _listeners = listeners;
            CallbackManager.OnAttached(this);
        }

        #endregion

        #region Properties

        public IViewModelPresenterCallbackManager CallbackManager { get; }

        public IComponentCollection<IChildViewModelPresenter> Presenters
        {
            get
            {
                if (_presenters == null)
                    MugenExtensions.LazyInitialize(ref _presenters, this);
                return _presenters;
            }
        }

        public IComponentCollection<IViewModelPresenterListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = ShowInternal(metadata);
                if (result == null)
                    ExceptionManager.ThrowPresenterCannotShowRequest(metadata.Dump());

                return OnShownInternal(metadata, result!);
            }
        }

        public IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = TryCloseInternal(metadata);
                return OnClosedInternal(metadata, result);
            }
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = TryRestoreInternal(metadata);
                return OnRestoredInternal(metadata, result);
            }
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                var presenter = presenters[i];
                if (!CanShow(presenter, metadata))
                    continue;

                var operation = presenter.TryShow(this, metadata);
                if (operation != null)
                    return operation;
            }

            return null;
        }

        protected virtual IViewModelPresenterResult OnShownInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult result)
        {
            var r = result as IViewModelPresenterResult;
            if (r == null)
            {
                var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                if (viewModel == null)
                    ExceptionManager.ThrowPresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

                var showingCallback = CallbackManager.AddCallback(viewModel!, NavigationCallbackType.Showing, result, metadata);
                var closeCallback = CallbackManager.AddCallback(viewModel!, NavigationCallbackType.Close, result, metadata);

                r = new ViewModelPresenterResult(viewModel!, showingCallback, closeCallback, result);
            }

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnShown(this, metadata, r);

            return r;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var results = new List<IChildViewModelPresenterResult>();
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                var presenter = presenters[i];
                if (!CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(this, metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        protected virtual IReadOnlyList<IClosingViewModelPresenterResult> OnClosedInternal(IReadOnlyMetadataContext metadata, IReadOnlyList<IChildViewModelPresenterResult> results)
        {
            var r = new List<IClosingViewModelPresenterResult>();
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (result is IClosingViewModelPresenterResult closingViewModelPresenterResult)
                    r.Add(closingViewModelPresenterResult);
                else
                {
                    var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                    if (viewModel == null)
                        ExceptionManager.ThrowPresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

                    var callback = CallbackManager.AddCallback(viewModel!, NavigationCallbackType.Closing, result, metadata);
                    r.Add(new ClosingViewModelPresenterResult((INavigationCallback<bool>)callback, result));
                }
            }


            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnClosed(this, metadata, r);

            return r;
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter && CanRestore(presenter, metadata))
                {
                    var result = presenter.TryRestore(this, metadata);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        protected virtual IRestorationViewModelPresenterResult OnRestoredInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult? result)
        {
            var r = result == null
                ? RestorationViewModelPresenterResult.Unrestored
                : result as IRestorationViewModelPresenterResult ?? new RestorationViewModelPresenterResult(true, result);
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnRestored(this, metadata, r);

            return r;
        }

        protected virtual bool CanShow(IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canShow = (listeners[i] as IConditionViewModelPresenterListener)?.CanShow(this, childPresenter, metadata) ?? true;
                if (!canShow)
                    return false;
            }

            return true;
        }

        protected virtual bool CanClose(IChildViewModelPresenter childPresenter, IReadOnlyList<IChildViewModelPresenterResult> currentResults, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canClose = (listeners[i] as IConditionViewModelPresenterListener)?.CanClose(this, childPresenter, currentResults, metadata) ?? true;
                if (!canClose)
                    return false;
            }

            return true;
        }

        protected virtual bool CanRestore(IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canRestore = (listeners[i] as IConditionViewModelPresenterListener)?.CanRestore(this, childPresenter, metadata) ?? true;
                if (!canRestore)
                    return false;
            }

            return true;
        }

        protected IReadOnlyList<IViewModelPresenterListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IViewModelPresenterListener>();
        }

        #endregion
    }
}