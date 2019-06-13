using System;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingObserverProvider
    {
        IComponentCollection<IChildBindingObserverProvider> Providers { get; }

        bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer);

        IBindingPath GetBindingPath(object path, IReadOnlyMetadataContext metadata);

        IBindingPathObserver GetBindingPathObserver(object source, object path, IReadOnlyMetadataContext metadata);
    }
}