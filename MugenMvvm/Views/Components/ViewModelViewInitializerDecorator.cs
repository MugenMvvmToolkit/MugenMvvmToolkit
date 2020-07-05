using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewInitializerDecorator : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelViewInitializerDecorator(IViewModelManager? viewModelManager = null, IServiceProvider? serviceProvider = null)
        {
            _viewModelManager = viewModelManager;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProviderDecorator;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync<TRequest>(IViewManager viewManager, IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                if (viewModel == null || view == null)
                    return Components.TryInitializeAsync(viewManager, mapping, ToViewModelViewRequest(mapping, viewModel, view, metadata), cancellationToken, metadata);
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
            }
            catch (Exception e)
            {
                return Task.FromException<IView>(e);
            }
        }

        public Task? TryCleanupAsync<TRequest>(IViewManager viewManager, IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);
        }

        #endregion

        #region Methods

        private ViewModelViewRequest ToViewModelViewRequest(IViewMapping mapping, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext? metadata)
        {
            return new ViewModelViewRequest(viewModel ?? _viewModelManager.DefaultIfNull().TryGetViewModel(mapping.ViewModelType, metadata),
                view ?? _serviceProvider.DefaultIfNull().GetService(mapping.ViewType));
        }

        #endregion
    }
}