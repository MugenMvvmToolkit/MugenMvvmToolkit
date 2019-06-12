using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingMemberObserver
    {
        IDisposable? TryObserve(object? source, object member, IBindingEventListener listener, IReadOnlyMetadataContext metadata);
    }
}