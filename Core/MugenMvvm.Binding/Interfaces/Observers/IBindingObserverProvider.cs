using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingObserverProvider : IComponentOwner<IBindingObserverProvider>, IComponent<IBindingManager>
    {
        BindingMemberObserver GetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null);

        IBindingPath GetBindingPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null);

        IBindingPathObserver GetBindingPathObserver<TPath>(object source, in TPath path, IReadOnlyMetadataContext? metadata = null);
    }
}