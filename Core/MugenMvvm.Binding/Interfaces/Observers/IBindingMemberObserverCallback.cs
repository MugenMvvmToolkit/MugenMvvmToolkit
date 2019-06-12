using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingMemberObserverCallback
    {
        IDisposable? TryObserve(object? target, object member, IBindingEventListener listener, IReadOnlyMetadataContext metadata);
    }
}