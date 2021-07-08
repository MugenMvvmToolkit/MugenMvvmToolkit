using System;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelProvider : IViewModelProviderComponent, IHasPriority
    {
        private readonly IServiceProvider? _serviceProvider;

        [Preserve(Conditional = true)]
        public ViewModelProvider(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        public int Priority { get; init; } = ViewModelComponentPriority.Provider;

        public IViewModelBase? TryGetViewModel(IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is not Type type)
                return null;

            var viewModel = (IViewModelBase?)_serviceProvider.DefaultIfNull().GetService(type, metadata);
            if (viewModel != null && !viewModel.IsInState(ViewModelLifecycleState.Initialized, metadata, viewModelManager))
            {
                viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initializing, request, metadata);
                viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Initialized, request, metadata);
            }

            return viewModel;
        }
    }
}