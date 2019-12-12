using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewProviderComponent : IComponent<IViewManager>
    {
        IReadOnlyList<IView>? TryGetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata);
    }
}