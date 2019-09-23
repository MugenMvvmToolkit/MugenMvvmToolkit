using System;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberObserverProviderComponent : IComponent<IObserverProvider>
    {
    }

    public interface IMemberObserverProviderComponent<TMember> : IMemberObserverProviderComponent
    {
        MemberObserver TryGetMemberObserver(Type type, in TMember member, IReadOnlyMetadataContext? metadata);
    }
}