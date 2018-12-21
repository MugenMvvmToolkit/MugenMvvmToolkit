using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IViewModelDispatcher
    {
        IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyContext context);

        IMessenger GetMessenger(IViewModel viewModel, IReadOnlyContext context);

        IContext GetMetadataContext(IViewModel viewModel, IReadOnlyContext context);

        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyContext context);

        [Pure]        
        IViewModel? TryGetViewModelById(Guid id, IReadOnlyContext context);
    }
}