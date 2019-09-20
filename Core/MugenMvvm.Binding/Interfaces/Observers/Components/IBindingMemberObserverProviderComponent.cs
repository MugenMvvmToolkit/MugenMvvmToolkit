using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IBindingMemberObserverProviderComponent : IComponent<IBindingObserverProvider>
    {
    }

    public interface IBindingMemberObserverProviderComponent<TMember> : IBindingMemberObserverProviderComponent
    {
        BindingMemberObserver TryGetMemberObserver(Type type, in TMember member, IReadOnlyMetadataContext? metadata);
    }
}