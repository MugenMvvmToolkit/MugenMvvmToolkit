using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Requests;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IViewManager? _viewManager;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewPresenterDecorator(IViewManager? viewManager = null, IViewModelManager? viewModelManager = null, INavigationDispatcher? navigationDispatcher = null,
            int priority = PresenterComponentPriority.ViewPresenterDecorator)
            : base(priority)
        {
            _viewManager = viewManager;
            _viewModelManager = viewModelManager;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public bool DisposeViewModelOnClose { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = TryGetViewModel(request, cancellationToken, metadata, out var view, out var canDisposeViewModel);
            if (viewModel == null)
                return Components.TryShow(presenter, request, cancellationToken, metadata);
            var result = Components.TryShow(presenter, ViewModelViewRequest.GetRequestOrRaw(request, viewModel, view), cancellationToken, metadata);
            if (canDisposeViewModel && DisposeViewModelOnClose)
            {
                foreach (var presenterResult in result)
                {
                    foreach (var callback in _navigationDispatcher.DefaultIfNull().GetNavigationCallbacks(presenterResult, metadata))
                    {
                        if (callback.CallbackType == NavigationCallbackType.Close)
                        {
                            callback.AddCallback(NavigationCallbackDelegateListener.DisposeTargetCallback);
                            break;
                        }
                    }
                }
            }

            if (result.IsEmpty && canDisposeViewModel)
                (viewModel as IDisposable)?.Dispose();

            return result;
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var vm = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (vm == null && view != null)
            {
                var views = _viewManager.DefaultIfNull().GetViews(request, metadata);
                var result = ItemOrListEditor.Get<IPresenterResult>();
                foreach (var v in views)
                    result.AddRange(Components.TryClose(presenter, v.ViewModel, cancellationToken, metadata));

                if (views.Count != 0)
                    return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
            }

            return Components.TryClose(presenter, request, cancellationToken, metadata);
        }

        #endregion

        #region Methods

        private IViewModelBase? TryGetViewModel(object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, out object? view, out bool canDisposeViewModel)
        {
            view = null;
            canDisposeViewModel = true;
            if (cancellationToken.IsCancellationRequested)
                return null;

            var viewModel = MugenExtensions.TryGetViewModelView(request, out view);
            if (viewModel != null || view == null)
                return null;

            var viewManager = _viewManager.DefaultIfNull();
            var views = viewManager.GetViews(view, metadata);
            if (!views.IsEmpty)
            {
                canDisposeViewModel = false;
                return views.Item?.ViewModel;
            }

            var mappings = viewManager.GetMappings(request, metadata);
            if (mappings.Item == null)
                return null;
            return _viewModelManager.DefaultIfNull().TryGetViewModel(mappings.Item.ViewModelType, metadata);
        }

        #endregion
    }
}