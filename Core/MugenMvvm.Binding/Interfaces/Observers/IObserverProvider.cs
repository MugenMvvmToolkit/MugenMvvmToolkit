using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IObserverProvider
    {
        IComponentCollection<IChildObserverProvider> Providers { get; }

        IBindingMemberObserver? TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata);
    }
}