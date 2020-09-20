using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILifecycleTrackerComponent<in T> : IComponent
    {
        bool IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata);
    }
}