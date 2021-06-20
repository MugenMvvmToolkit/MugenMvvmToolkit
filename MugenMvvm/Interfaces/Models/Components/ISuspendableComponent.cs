using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models.Components
{
    public interface ISuspendableComponent<T> : IComponent<T> where T : class
    {
        bool IsSuspended(T owner, IReadOnlyMetadataContext? metadata);

        ActionToken TrySuspend(T owner, object? state, IReadOnlyMetadataContext? metadata);
    }
}