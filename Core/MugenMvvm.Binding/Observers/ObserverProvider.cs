using System;
using MugenMvvm.Attributes;
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
            var providers = GetComponents<IMemberObserverProviderComponent>(metadata);
            for (var i = 0; i < providers.Length; i++)
            {
                var observer = providers[i].TryGetMemberObserver(type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        public IMemberPath GetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            if (path is IMemberPath p)
                return p;

            var providers = GetComponents<IMemberPathProviderComponent>(metadata);
            for (var i = 0; i < providers.Length; i++)
            {
                var memberPath = providers[i].TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, providers);
            return null;
        }

        public IMemberPathObserver GetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var providers = GetComponents<IMemberPathObserverProviderComponent>(metadata);
            for (var i = 0; i < providers.Length; i++)
            {
                var observer = providers[i].TryGetMemberPathObserver(target, request, metadata);
                if (observer != null)
                    return observer;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, providers);
            return null;
        }

        #endregion
    }
}