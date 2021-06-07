using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Tests.ViewModels
{
    public class TestViewModelServiceProviderComponent : IViewModelServiceProviderComponent, IHasPriority
    {
        public Func<IViewModelManager, IViewModelBase, object, IReadOnlyMetadataContext?, object?>? TryGetService { get; set; }

        public int Priority { get; set; }

        object? IViewModelServiceProviderComponent.
            TryGetService(IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetService?.Invoke(viewModelManager, viewModel, request, metadata);
    }
}