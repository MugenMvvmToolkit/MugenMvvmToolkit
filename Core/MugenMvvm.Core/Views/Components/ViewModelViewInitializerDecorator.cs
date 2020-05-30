using System;
using System.Diagnostics.CodeAnalysis;
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
using MugenMvvm.Internal;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewInitializerDecorator : ComponentDecoratorBase<IViewManager, IViewInitializerComponent>, IViewInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewModelViewInitializerDecorator(IViewModelManager? viewModelManager = null, IServiceProvider? serviceProvider = null)
        {
            _viewModelManager = viewModelManager;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync<TRequest>(IViewModelViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (typeof(TRequest) == typeof(ViewModelViewRequest))
                {
                    var r = MugenExtensions.CastGeneric<TRequest, ViewModelViewRequest>(request);
                    return Components.TryInitializeAsync(mapping, ToViewModelViewRequest(mapping, r.ViewModel, r.View, metadata), cancellationToken, metadata);
                }

                if (!Default.IsValueType<TRequest>())
                {
                    if (request is IViewModelBase viewModel)
                        return Components.TryInitializeAsync(mapping, ToViewModelViewRequest(mapping, viewModel, null, metadata), cancellationToken, metadata);
                    if (mapping.ViewType.IsInstanceOfType(request))
                        return Components.TryInitializeAsync(mapping, ToViewModelViewRequest(mapping, null, request, metadata), cancellationToken, metadata);
                }

                return Components.TryInitializeAsync(mapping, request, cancellationToken, metadata);
            }
            catch (Exception e)
            {
                return Task.FromException<IView>(e);
            }
        }

        public Task? TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Components.TryCleanupAsync(view, request, cancellationToken, metadata);
        }

        #endregion

        #region Methods

        private ViewModelViewRequest ToViewModelViewRequest(IViewModelViewMapping mapping, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext? metadata)
        {
            return new ViewModelViewRequest(viewModel ?? _viewModelManager.DefaultIfNull().TryGetViewModel(mapping.ViewModelType, metadata),
                view ?? _serviceProvider.DefaultIfNull().GetService(mapping.ViewType));
        }

        #endregion
    }
}