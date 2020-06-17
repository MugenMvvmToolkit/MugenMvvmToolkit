using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IObserverProvider : IComponentOwner<IObserverProvider>, IComponent<IMugenApplication>
    {
        IMemberPath? TryGetMemberPath<TPath>([DisallowNull] in TPath path, IReadOnlyMetadataContext? metadata = null);

        MemberObserver TryGetMemberObserver<TMember>(Type type, [DisallowNull] in TMember member, IReadOnlyMetadataContext? metadata = null);

        IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}