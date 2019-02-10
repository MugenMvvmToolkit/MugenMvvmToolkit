using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface ICallbackProviderNavigationDispatcherListener : INavigationDispatcherListener
    {
        IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}