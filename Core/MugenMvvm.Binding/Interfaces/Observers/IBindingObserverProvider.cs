using System;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingObserverProvider : IComponentOwner<IBindingObserverProvider>
    {
        BindingMemberObserver GetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null);

        IBindingPath GetBindingPath(object path, IReadOnlyMetadataContext? metadata = null);

        IBindingPathObserver GetBindingPathObserver(object source, object path, IReadOnlyMetadataContext? metadata = null);
    }
}