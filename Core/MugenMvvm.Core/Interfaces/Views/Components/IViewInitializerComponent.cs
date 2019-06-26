using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewInitializerComponent : IComponent<IViewManager>
    {
        IViewInitializerResult? TryInitialize(IViewInitializer initializer, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext? TryCleanup(IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}