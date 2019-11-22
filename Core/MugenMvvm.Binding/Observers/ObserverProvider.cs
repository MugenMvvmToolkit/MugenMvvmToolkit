using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class ObserverProvider : ComponentOwnerBase<IObserverProvider>, IObserverProvider,
        IComponentOwnerAddedCallback<IComponent<IObserverProvider>>, IComponentOwnerRemovedCallback<IComponent<IObserverProvider>>
    {
        #region Fields

        private IMemberObserverProviderComponent[] _memberObserverProviders;
        private IMemberPathObserverProviderComponent[] _pathObserverProviders;
        private IMemberPathProviderComponent[] _pathProviders;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObserverProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _memberObserverProviders = Default.EmptyArray<IMemberObserverProviderComponent>();
            _pathObserverProviders = Default.EmptyArray<IMemberPathObserverProviderComponent>();
            _pathProviders = Default.EmptyArray<IMemberPathProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IObserverProvider>>.OnComponentAdded(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _memberObserverProviders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _pathObserverProviders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _pathProviders, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IObserverProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberObserverProviders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _pathObserverProviders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _pathProviders, component);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            var providers = _memberObserverProviders;
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

            var providers = _pathProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var memberPath = providers[i].TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberPathProviderComponent).Name);
            return null;
        }

        public IMemberPathObserver GetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var providers = _pathObserverProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var observer = providers[i].TryGetMemberPathObserver(target, request, metadata);
                if (observer != null)
                    return observer;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberObserverProviderComponent).Name);
            return null;
        }

        #endregion
    }
}