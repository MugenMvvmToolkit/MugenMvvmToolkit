using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewProviderInitializerComponent : DecoratorComponentBase<IViewManager, IViewInitializerComponent>, IViewInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewModelViewProviderInitializerComponent(IViewModelManager? viewModelManager = null, IServiceProvider? serviceProvider = null)
        {
            _viewModelManager = viewModelManager;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewModelViewProvider;

        #endregion

        #region Implementation of interfaces

        public Task<ViewInitializationResult>? TryInitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(new ViewModelProviderRequest(mapping.ViewModelType), metadata);
            if (view == null)
                view = (_serviceProvider ?? MugenService.Instance<IServiceProvider>()).GetService(mapping.ViewType);
            return Components.TryInitializeAsync(mapping, view, viewModel, metadata, cancellationToken);
        }

        public Task? TryCleanupAsync(IView view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return Components.TryCleanupAsync(view, viewModel, metadata, cancellationToken);
        }

        #endregion
    }
}