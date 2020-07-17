using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackManagerComponent : IComponent<INavigationDispatcher>
    {
        INavigationCallback? TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, object request, IReadOnlyMetadataContext? metadata);

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception);

        bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}