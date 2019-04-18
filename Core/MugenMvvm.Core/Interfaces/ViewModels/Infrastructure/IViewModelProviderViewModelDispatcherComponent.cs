using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelProviderViewModelDispatcherComponent : IViewModelDispatcherComponent
    {
        IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Type viewModelType, IReadOnlyMetadataContext metadata);

        IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata);
    }
}