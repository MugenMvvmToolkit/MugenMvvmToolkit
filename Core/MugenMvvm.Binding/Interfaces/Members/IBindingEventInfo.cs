using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingEventInfo : IBindingMemberInfo
    {
        IDisposable? TrySubscribe(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null);
    }
}