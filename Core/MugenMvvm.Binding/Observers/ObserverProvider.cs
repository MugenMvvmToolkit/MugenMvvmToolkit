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

        public MemberObserver GetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            return GetMemberObserverInternal(type, member, metadata);
        }

        public IMemberPath GetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            var p = path as IMemberPath ?? TryGetMemberPathInternal(path, metadata);
            if (p == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberPathProviderComponent<TPath>).Name);
            return p!;
        }

        public IMemberPathObserver GetMemberPathObserver<TPath>(object source, in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(source, nameof(source));
            var memberPath = GetMemberPath(path, metadata);
            var observer = TryGetMemberPathObserverInternal(source, memberPath, metadata);
            if (observer == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMemberObserverProviderComponent).Name);
            return observer!;
        }

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

        #endregion

        #region Methods

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref MemberObserverProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref PathObserverProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref PathProviders, this, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IObserverProvider>> collection,
            IComponent<IObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref MemberObserverProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathObserverProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathProviders, collection, component, metadata);
        }

        protected virtual MemberObserver GetMemberObserverInternal<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < MemberObserverProviders.Length; i++)
            {
                if (MemberObserverProviders[i] is IMemberObserverProviderComponent<TMember> component)
                {
                    var observer = component.TryGetMemberObserver(type, member, metadata);
                    if (!observer.IsEmpty)
                        return observer;
                }
            }

            return default;
        }

        protected virtual IMemberPath? TryGetMemberPathInternal<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < PathProviders.Length; i++)
            {
                var memberPath = (PathProviders[i] as IMemberPathProviderComponent<TPath>)?.TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        protected virtual IMemberPathObserver? TryGetMemberPathObserverInternal(object source, IMemberPath path, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < PathObserverProviders.Length; i++)
            {
                var observer = PathObserverProviders[i].TryGetMemberPathObserver(source, path, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}