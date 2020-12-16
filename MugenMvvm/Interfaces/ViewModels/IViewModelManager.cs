using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModelManager : IComponentOwner<IViewModelManager>
    {
        bool IsInState(IViewModelBase viewModel, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null);

        object? TryGetService(IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata = null);

        IViewModelBase? TryGetViewModel(object request, IReadOnlyMetadataContext? metadata = null);
    }
}