using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewMappingProviderComponent : IComponent<IViewManager>
    {
        ItemOrIReadOnlyList<IViewMapping> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata);
    }
}