using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherListener
    {
        IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata);

        void OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);

        void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);
    }
}