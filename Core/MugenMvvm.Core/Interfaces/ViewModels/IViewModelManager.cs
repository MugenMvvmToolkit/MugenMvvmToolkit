using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    //todo cleanup manager, clear commands, initialize manager, provider manager
    public interface IViewModelManager : IComponentOwner<IViewModelManager>, IComponent<IMugenApplication>
    {
        void OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, [AllowNull] in TState state, IReadOnlyMetadataContext? metadata = null);

        object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata = null);

        IViewModelBase? TryGetViewModel<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}