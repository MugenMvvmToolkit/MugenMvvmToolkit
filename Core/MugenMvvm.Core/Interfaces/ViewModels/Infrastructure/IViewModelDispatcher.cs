using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcher : IHasListeners<IViewModelDispatcherListener>//todo cleanup listener, clear commands, listeners etc, rename listener to manager
    {
        [Pure]
        object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata);

        void AddServiceResolver(IViewModelDispatcherServiceResolver resolver);

        void RemoveServiceResolver(IViewModelDispatcherServiceResolver resolver);

        IReadOnlyList<IViewModelDispatcherServiceResolver> GetServiceResolvers();

        bool Subscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        bool Unsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);

        void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);

        IViewModelBase? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata);
    }
}