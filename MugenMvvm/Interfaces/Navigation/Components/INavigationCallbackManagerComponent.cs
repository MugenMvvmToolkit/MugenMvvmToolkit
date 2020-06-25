using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackManagerComponent : IComponent<INavigationDispatcher>
    {
        INavigationCallback? TryAddNavigationCallback<TRequest>(NavigationCallbackType callbackType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> TryGetNavigationCallbacks<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext);

        bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception);

        bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}