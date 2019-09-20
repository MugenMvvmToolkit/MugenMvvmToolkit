using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewInitializerProviderComponent : IComponent<IViewManager>
    {
        IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}