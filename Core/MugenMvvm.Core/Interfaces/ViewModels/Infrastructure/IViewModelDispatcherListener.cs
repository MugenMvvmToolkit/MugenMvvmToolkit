using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherListener
    {
        IViewModel? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata);

        void OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, object observer, IReadOnlyMetadataContext metadata);

        void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);
    }
}