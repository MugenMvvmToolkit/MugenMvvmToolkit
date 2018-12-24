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
        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);

        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);

        IMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadataContext);

        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadataContext);

        [Pure]
        IViewModel? TryGetViewModelById(Guid id, IReadOnlyMetadataContext metadataContext);
    }
}