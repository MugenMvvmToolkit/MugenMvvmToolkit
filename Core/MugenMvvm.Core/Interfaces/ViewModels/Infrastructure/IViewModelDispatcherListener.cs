using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherListener : IListener
    {
        IViewModelBase? TryGetViewModel(IViewModelDispatcher viewModelDispatcher, Guid id, IReadOnlyMetadataContext metadata);

        bool OnSubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        bool OnUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);

        void OnLifecycleChanged(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);
    }
}