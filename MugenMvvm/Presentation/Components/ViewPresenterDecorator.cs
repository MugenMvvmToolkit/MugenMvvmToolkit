﻿using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;
using MugenMvvm.Requests;

namespace MugenMvvm.Presentation.Components
{
    public sealed class ViewPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IViewManager? _viewManager;
        private readonly IViewModelManager? _viewModelManager;

        public ViewPresenterDecorator(IViewManager? viewManager = null, IViewModelManager? viewModelManager = null, INavigationDispatcher? navigationDispatcher = null,
            int priority = PresenterComponentPriority.ViewPresenterDecorator)
            : base(priority)
        {
            _viewManager = viewManager;
            _viewModelManager = viewModelManager;
            _navigationDispatcher = navigationDispatcher;
        }

        public bool DisposeViewModelOnClose { get; set; } = true;

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = TryGetViewModel(request, cancellationToken, metadata, out var view, out var canDisposeViewModel);
            if (viewModel == null)
                return Components.TryShow(presenter, request, cancellationToken, metadata);
            var result = Components.TryShow(presenter, ViewModelViewRequest.GetRequestOrRaw(request, viewModel, view), cancellationToken, metadata);
            if (canDisposeViewModel && DisposeViewModelOnClose)
            {
                foreach (var presenterResult in result)
                foreach (var callback in _navigationDispatcher.DefaultIfNull(viewModel).GetNavigationCallbacks(presenterResult, metadata))
                {
                    if (callback.CallbackType == NavigationCallbackType.Close)
                    {
                        callback.AddCallback(NavigationCallbackDelegateListener.DisposeTargetCallback);
                        break;
                    }
                }
            }

            if (result.IsEmpty && canDisposeViewModel)
                (viewModel as IDisposable)?.Dispose();

            return result;
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var vm = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (vm == null && view != null)
            {
                var views = _viewManager.DefaultIfNull().GetViews(request, metadata);
                var result = new ItemOrListEditor<IPresenterResult>(2);
                foreach (var v in views)
                    result.AddRange(Components.TryClose(presenter, v.ViewModel, cancellationToken, metadata));

                if (views.Count != 0)
                    return result.ToItemOrList();
            }

            return Components.TryClose(presenter, request, cancellationToken, metadata);
        }

        private IViewModelBase? TryGetViewModel(object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, out object? view,
            out bool canDisposeViewModel)
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
    }
}