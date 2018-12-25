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
        IViewModel? GetViewModelById(Guid id, IReadOnlyMetadataContext metadataContext);


        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);

        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);
        
        IObservableMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);


        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadataContext);        
    }
}