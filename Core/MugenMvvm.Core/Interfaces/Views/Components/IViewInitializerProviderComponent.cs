using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewInitializerProviderComponent : IComponent<IViewManager>
    {
        IReadOnlyList<IViewInitializer> TryGetInitializersByView(object view, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IViewInitializer> TryGetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}