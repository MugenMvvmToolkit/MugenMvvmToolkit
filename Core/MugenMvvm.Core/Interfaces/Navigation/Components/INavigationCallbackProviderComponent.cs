using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackProviderComponent : IComponent<INavigationDispatcher>
    {
        IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null);
    }
}