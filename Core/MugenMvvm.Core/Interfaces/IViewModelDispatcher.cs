using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IViewModelDispatcher
    {
        [Pure]
        IViewModel? GetViewModelById(Guid id, IReadOnlyMetadataContext metadata);


        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        IObservableMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadata);


        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);
    }
}