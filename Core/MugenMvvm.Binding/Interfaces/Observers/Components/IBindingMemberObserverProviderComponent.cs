using System;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IBindingMemberObserverProviderComponent : IComponent<IBindingObserverProvider>
    {
        BindingMemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata);
    }
}