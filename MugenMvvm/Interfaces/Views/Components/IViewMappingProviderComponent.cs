using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewMappingProviderComponent : IComponent<IViewManager>
    {
        ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata);
    }
}