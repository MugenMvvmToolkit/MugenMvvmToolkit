using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelManager : IComponentOwner<IViewModelManager>
    {
        void OnLifecycleChanged<TState>(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null);

        object? TryGetService<TRequest>(IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IViewModelBase? TryGetViewModel<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}