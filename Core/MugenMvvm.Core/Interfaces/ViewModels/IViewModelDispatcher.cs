using System;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelDispatcher : IComponentOwner<IViewModelDispatcher>, IComponent<IMugenApplication> //todo cleanup manager, clear commands, initialize manager, provider manager
    {
        IReadOnlyMetadataContext OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata = null);

        [Pure]
        object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata = null);

        [Pure]
        IViewModelBase? TryGetViewModel(IReadOnlyMetadataContext metadata);

        bool Subscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        bool Unsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext? metadata = null);
    }
}