using System;
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

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (mapping != ViewMapping.Undefined)
                {
                    var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                    if (viewModel == null || view == null)
                        request = ToViewModelViewRequest(mapping, request, viewModel, view, metadata);
                }

                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
            }
            catch (Exception e)
            {
                return Task.FromException<IView>(e);
            }
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);

        #endregion

        #region Methods

        private object ToViewModelViewRequest(IViewMapping mapping, object request, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext? metadata) =>
            ViewModelViewRequest.GetRequestOrRaw(request, viewModel ?? _viewModelManager.DefaultIfNull().TryGetViewModel(mapping.ViewModelType, metadata),
                view ?? _serviceProvider.DefaultIfNull().GetService(mapping.ViewType));

        #endregion
    }
}