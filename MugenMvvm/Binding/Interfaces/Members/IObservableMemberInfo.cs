using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IObservableMemberInfo : IMemberInfo
    {
        ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}