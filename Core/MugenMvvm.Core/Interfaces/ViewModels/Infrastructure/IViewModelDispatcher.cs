using System;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcher : IHasListeners<IViewModelDispatcherListener>//todo resolve service, add resolver for service, move all code to mediators
    {
        [Pure]
        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        [Pure]
        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        [Pure]
        IObservableMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        void Subscribe(IViewModel viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void Unsubscribe(IViewModel viewModel, object observer, IReadOnlyMetadataContext metadata);

        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);

        IViewModel? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata);
    }
}