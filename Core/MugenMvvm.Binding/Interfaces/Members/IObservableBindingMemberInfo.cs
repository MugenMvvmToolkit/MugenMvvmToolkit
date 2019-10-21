using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IObservableBindingMemberInfo : IBindingMemberInfo
    {
        Unsubscriber TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}