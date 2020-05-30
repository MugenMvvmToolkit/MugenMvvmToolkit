using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackManagerComponent : IComponent<INavigationDispatcher>
    {
        INavigationCallback? TryAddNavigationCallback<TRequest>(NavigationCallbackType callbackType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, [DisallowNull] in TRequest request, Exception exception, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}