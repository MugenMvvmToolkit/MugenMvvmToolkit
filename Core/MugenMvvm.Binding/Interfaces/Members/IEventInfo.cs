using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IEventInfo : IMemberInfo
    {
        ActionToken TrySubscribe(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}