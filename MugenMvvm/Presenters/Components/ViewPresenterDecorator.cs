using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
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
    public sealed class ViewPresenterDecorator : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent, IHasPriority//todo test
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IViewManager? _viewManager;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewPresenterDecorator(IViewManager? viewManager = null, IViewModelManager? viewModelManager = null, INavigationDispatcher? navigationDispatcher = null)
        {
            _viewManager = viewManager;
            _viewModelManager = viewModelManager;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = PresenterComponentPriority.ViewModelProviderDecorator;

        public bool DisposeViewModelOnClose { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>(IPresenter presenter, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = TryGetViewModel(request, cancellationToken, metadata, out var view);
            if (viewModel == null)
                return Components.TryShow(presenter, request, cancellationToken, metadata);
            var result = Components.TryShow(presenter, new ViewModelViewRequest(viewModel, view), cancellationToken, metadata);
            if (DisposeViewModelOnClose)
            {
                for (var i = 0; i < result.Count(); i++)
                {
                    var callbacks = _navigationDispatcher
                        .DefaultIfNull()
                        .GetNavigationCallbacks(result.Get(i), metadata);
                    for (var j = 0; j < callbacks.Count(); j++)
                    {
                        var callback = callbacks.Get(j);
                        if (callback.CallbackType == NavigationCallbackType.Close)
                        {
                            callback.AddCallback(NavigationCallbackDelegateListener.DisposeTargetCallback);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>(IPresenter presenter, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryClose(presenter, request, cancellationToken, metadata);
        }

        #endregion

        #region Methods

        private IViewModelBase? TryGetViewModel<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, out object? view)
        {
            view = null;
            if (cancellationToken.IsCancellationRequested)
                return null;

            var viewModel = MugenExtensions.TryGetViewModelView(request, out view);
            if (viewModel != null || view == null)
                return null;

            var mappings = _viewManager.DefaultIfNull().GetMappings(request, metadata);
            if (mappings.Item == null)
                return null;
            return _viewModelManager.DefaultIfNull().TryGetViewModel(mappings.Item.ViewModelType, metadata);
        }

        #endregion
    }
}