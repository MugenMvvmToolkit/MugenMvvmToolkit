using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILifecycleTrackerComponent<in T> : IComponent where T : class, IEnum
    {
        bool IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata);
    }
}