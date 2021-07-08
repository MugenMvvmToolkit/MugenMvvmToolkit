using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackManagerComponent : IComponent<INavigationDispatcher>
    {
        INavigationCallback? TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId,
            NavigationType navigationType, object request, IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<INavigationCallback> TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            Exception exception);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            CancellationToken cancellationToken);
    }
}