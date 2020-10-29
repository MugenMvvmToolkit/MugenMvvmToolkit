using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IObservableMemberInfo : IMemberInfo
    {
        ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}