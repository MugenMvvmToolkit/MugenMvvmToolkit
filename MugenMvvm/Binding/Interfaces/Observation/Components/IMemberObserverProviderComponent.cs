using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation.Components
{
    public interface IMemberObserverProviderComponent : IComponent<IObservationManager>
    {
        MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull]in TMember member, IReadOnlyMetadataContext? metadata);
    }
}