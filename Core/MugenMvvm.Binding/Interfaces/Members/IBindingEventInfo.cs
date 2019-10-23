using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingEventInfo : IBindingMemberInfo
    {
        Unsubscriber TrySubscribe(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}