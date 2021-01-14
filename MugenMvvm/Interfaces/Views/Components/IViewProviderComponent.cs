using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewProviderComponent : IComponent<IViewManager>
    {
        ItemOrIReadOnlyList<IView> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata);
    }
}