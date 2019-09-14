using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public class BindingObserverProvider : ComponentOwnerBase<IBindingObserverProvider>, IBindingObserverProvider,
        IComponentOwnerAddedCallback<IComponent<IBindingObserverProvider>>, IComponentOwnerRemovedCallback<IComponent<IBindingObserverProvider>>
    {
        #region Fields

        protected IBindingMemberObserverProviderComponent[] MemberObserverProviders;
        protected IBindingPathObserverProviderComponent[] PathObserverProviders;
        protected IBindingPathProviderComponent[] PathProviders;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingObserverProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            MemberObserverProviders = Default.EmptyArray<IBindingMemberObserverProviderComponent>();
            PathObserverProviders = Default.EmptyArray<IBindingPathObserverProviderComponent>();
            PathProviders = Default.EmptyArray<IBindingPathProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public BindingMemberObserver GetMemberObserver(Type type, object member, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            return GetMemberObserverInternal(type, member, metadata);
        }

        public IBindingPath GetBindingPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            var p = path as IBindingPath ?? TryGetBindingPathInternal(path, metadata);
            if (p == null)
                ExceptionManager.ThrowNotSupported(nameof(path));
            return p!;
        }

        public IBindingPathObserver GetBindingPathObserver<TPath>(object source, in TPath path, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(source, nameof(source));
            var bindingPath = GetBindingPath(path, metadata);
            var observer = TryGetBindingPathObserverInternal(source, bindingPath, metadata);
            if (observer == null)
                ExceptionManager.ThrowNotSupported(nameof(IBindingPathObserver));
            return observer!;
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingObserverProvider>>.OnComponentAdded(IComponentCollection<IComponent<IBindingObserverProvider>> collection,
            IComponent<IBindingObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IBindingObserverProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IBindingObserverProvider>> collection,
            IComponent<IBindingObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IBindingObserverProvider>> collection,
            IComponent<IBindingObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref MemberObserverProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref PathObserverProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref PathProviders, this, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IBindingObserverProvider>> collection,
            IComponent<IBindingObserverProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref MemberObserverProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathObserverProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref PathProviders, collection, component, metadata);
        }

        protected virtual BindingMemberObserver GetMemberObserverInternal(Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < MemberObserverProviders.Length; i++)
            {
                var observer = MemberObserverProviders[i].TryGetMemberObserver(type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        protected virtual IBindingPath? TryGetBindingPathInternal<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < PathProviders.Length; i++)
            {
                var bindingPath = (PathProviders[i] as IBindingPathProviderComponent<TPath>)?.TryGetBindingPath(path, metadata);
                if (bindingPath != null)
                    return bindingPath;
            }

            return null;
        }

        protected virtual IBindingPathObserver? TryGetBindingPathObserverInternal(object source, IBindingPath path, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < PathObserverProviders.Length; i++)
            {
                var observer = PathObserverProviders[i].TryGetBindingPathObserver(source, path, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}