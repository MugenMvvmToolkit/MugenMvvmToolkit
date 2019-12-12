using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewModelViewMappingProviderComponent : IComponent<IViewManager>
    {
        IReadOnlyList<IViewModelViewMapping>? TryGetMappingByView(object view, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IViewModelViewMapping>? TryGetMappingByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}