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
        INavigationCallback? TryAddNavigationCallback<TTarget>(NavigationCallbackType callbackType, [DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TTarget>([DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, [DisallowNull] in TTarget target, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, [DisallowNull] in TTarget target, Exception exception, IReadOnlyMetadataContext? metadata);

        bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, [DisallowNull] in TTarget target, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}