using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewInfoProviderComponent : IComponent<IViewManager>
    {
        IReadOnlyList<IViewInfo> TryGetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}