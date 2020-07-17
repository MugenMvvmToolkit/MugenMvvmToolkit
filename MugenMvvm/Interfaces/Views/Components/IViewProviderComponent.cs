using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewProviderComponent : IComponent<IViewManager>
    {
        ItemOrList<IView, IReadOnlyList<IView>> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata);
    }
}