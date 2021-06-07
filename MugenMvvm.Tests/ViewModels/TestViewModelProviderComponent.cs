using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.Tests.ViewModels
{
    public class TestViewModelProviderComponent : IViewModelProviderComponent, IHasPriority
    {
        public Func<IViewModelManager, object, IReadOnlyMetadataContext?, IViewModelBase?>? TryGetViewModel { get; set; }

        public int Priority { get; set; }

        IViewModelBase? IViewModelProviderComponent.TryGetViewModel(IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetViewModel?.Invoke(viewModelManager, request, metadata);
    }
}