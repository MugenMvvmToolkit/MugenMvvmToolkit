using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewInitializerComponent : IComponent<IViewManager>
    {
        IViewInitializerResult? TryInitialize(IViewInitializer initializer, IViewModelBase viewModel, object view, IMetadataContext metadata);

        IReadOnlyMetadataContext? TryCleanup(IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata);
    }
}