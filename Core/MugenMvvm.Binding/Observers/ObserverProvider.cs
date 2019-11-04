using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public class ObserverProvider : ComponentOwnerBase<IObserverProvider>, IObserverProvider,
        IComponentOwnerAddedCallback<IComponent<IObserverProvider>>, IComponentOwnerRemovedCallback<IComponent<IObserverProvider>>
    {
        #region Fields

        protected IMemberObserverProviderComponent[] MemberObserverProviders;
        protected IMemberPathObserverProviderComponent[] PathObserverProviders;
        protected IMemberPathProviderComponent[] PathProviders;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ObserverProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            MemberObserverProviders = Default.EmptyArray<IMemberObserverProviderComponent>();
            PathObserverProviders = Default.EmptyArray<IMemberPathObserverProviderComponent>();
            PathProviders = Default.EmptyArray<IMemberPathProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IObserverProvider>>.OnComponentAdded(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IObserverProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            return GetMemberObserverInternal(type, member, metadata);
        }

        public IMemberPath GetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            var p = path as IMemberPath ?? TryGetMemberPathInternal(path, metadata);
            if (p == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberPathProviderComponent).Name);
            return p!;
        }

        public IMemberPathObserver GetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var observer = TryGetMemberPathObserverInternal(target, request, metadata);
            if (observer == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberObserverProviderComponent).Name);
            return observer!;
        }

        #endregion

        #region Methods

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref MemberObserverProviders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref PathObserverProviders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref PathProviders, collection, component);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref MemberObserverProviders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathObserverProviders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathProviders, component);
        }

        protected virtual IMemberPath? TryGetMemberPathInternal<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            var providers = PathProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var memberPath = providers[i].TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        protected virtual MemberObserver GetMemberObserverInternal<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            var providers = MemberObserverProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var observer = providers[i].TryGetMemberObserver(type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        protected virtual IMemberPathObserver? TryGetMemberPathObserverInternal<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            var providers = PathObserverProviders;
            for (var i = 0; i < providers.Length; i++)
            {
                var observer = providers[i].TryGetMemberPathObserver(target, request, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}