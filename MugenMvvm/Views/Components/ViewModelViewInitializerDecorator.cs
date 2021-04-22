using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewInitializerDecorator : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewModelManager? _viewModelManager;

        [Preserve(Conditional = true)]
        public ViewModelViewInitializerDecorator(IViewModelManager? viewModelManager = null, IServiceProvider? serviceProvider = null,
            int priority = ViewComponentPriority.ViewModelViewProviderDecorator)
            : base(priority)
        {
            _viewModelManager = viewModelManager;
            _serviceProvider = serviceProvider;
        }

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            if (!mapping.IsUndefined())
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                if (viewModel == null || view == null)
                    request = ToViewModelViewRequest(mapping, request, viewModel, view, metadata);
            }

            return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
        }

        public ValueTask<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            Components.TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);

        private object ToViewModelViewRequest(IViewMapping mapping, object request, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext? metadata) =>
            ViewModelViewRequest.GetRequestOrRaw(request, viewModel ?? _viewModelManager.DefaultIfNull().TryGetViewModel(mapping.ViewModelType, metadata),
                view ?? _serviceProvider.DefaultIfNull().GetService(mapping.ViewType, metadata));
    }
}