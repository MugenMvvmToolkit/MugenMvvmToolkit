using System;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcher //todo cleanup manager, clear commands, initialize manager, provider manager
    {
        IComponentCollection<IViewModelDispatcherComponent> Components { get; }

        IReadOnlyMetadataContext OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);

        [Pure]
        object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);

        bool Subscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        bool Unsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);

        [Pure]
        IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext metadata);

        [Pure]
        IViewModelBase? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata);
    }
}