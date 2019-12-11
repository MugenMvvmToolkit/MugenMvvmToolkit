using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class ObserverProvider : ComponentOwnerBase<IObserverProvider>, IObserverProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ObserverProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            return GetComponents<IMemberObserverProviderComponent>(metadata).TryGetMemberObserver(type, member, metadata);
        }

        public IMemberPath GetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMemberPathProviderComponent>(metadata).TryGetMemberPath(path, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public IMemberPathObserver GetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var result = GetComponents<IMemberPathObserverProviderComponent>(metadata).TryGetMemberPathObserver(target, request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        #endregion
    }
}