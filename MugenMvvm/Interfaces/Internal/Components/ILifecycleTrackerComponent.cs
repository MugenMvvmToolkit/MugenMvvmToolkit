using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILifecycleTrackerComponent<TContainer, in T> : IComponent<TContainer>
        where TContainer : class, IComponentOwner
        where T : class, IEnum
    {
        bool IsInState(TContainer owner, object target, T state, IReadOnlyMetadataContext? metadata);
    }
}