using System;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IObservationManager : IComponentOwner<IObservationManager>
    {
        IMemberPath? TryGetMemberPath(object path, IReadOnlyMetadataContext? metadata = null);

        MemberObserver TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null);

        IMemberPathObserver? TryGetMemberPathObserver(object target, object request, IReadOnlyMetadataContext? metadata = null);
    }
}