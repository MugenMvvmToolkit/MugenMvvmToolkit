using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IObservableMemberInfo : IMemberInfo
    {
        ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}