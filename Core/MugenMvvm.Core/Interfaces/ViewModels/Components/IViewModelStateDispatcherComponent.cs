using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface IViewModelStateDispatcherComponent : IComponent<IViewModelManager>
    {
        void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IMetadataContext dispatcherMetadata, IReadOnlyMetadataContext? metadata);
    }
}