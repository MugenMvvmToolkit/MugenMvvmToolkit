using System;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IObserverProvider
    {
        IComponentCollection<IChildObserverProvider> Providers { get; }

        bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer);
    }
}