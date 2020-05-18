using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationCallbackProviderComponent : IComponent<INavigationDispatcher>
    {
        IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TTarget>([DisallowNull]in TTarget target, IReadOnlyMetadataContext? metadata);
    }
}