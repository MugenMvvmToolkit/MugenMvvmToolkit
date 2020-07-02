using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IObservationManager : IComponentOwner<IObservationManager>
    {
        IMemberPath? TryGetMemberPath<TPath>([DisallowNull] in TPath path, IReadOnlyMetadataContext? metadata = null);

        MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull] in TMember member, IReadOnlyMetadataContext? metadata = null);

        IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}