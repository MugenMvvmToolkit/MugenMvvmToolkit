using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class ReflectionDelegateProvider : ComponentOwnerBase<IReflectionDelegateProvider>, IReflectionDelegateProvider,
        IComponentOwnerAddedCallback<IComponent<IReflectionDelegateProvider>>, IComponentOwnerRemovedCallback<IComponent<IReflectionDelegateProvider>>
    {
        #region Fields

        private IActivatorReflectionDelegateProviderComponent[] _activatorComponents;
        private IReflectionDelegateProviderComponent[] _delegateComponents;
        private IMemberReflectionDelegateProviderComponent[] _memberComponents;
        private IMethodReflectionDelegateProviderComponent[] _methodComponents;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionDelegateProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _delegateComponents = Default.EmptyArray<IReflectionDelegateProviderComponent>();
            _activatorComponents = Default.EmptyArray<IActivatorReflectionDelegateProviderComponent>();
            _methodComponents = Default.EmptyArray<IMethodReflectionDelegateProviderComponent>();
            _memberComponents = Default.EmptyArray<IMemberReflectionDelegateProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IReflectionDelegateProvider>>.OnComponentAdded(IComponentCollection<IComponent<IReflectionDelegateProvider>> collection,
            IComponent<IReflectionDelegateProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _delegateComponents, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _activatorComponents, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _methodComponents, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref _memberComponents, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IReflectionDelegateProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IReflectionDelegateProvider>> collection,
            IComponent<IReflectionDelegateProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _delegateComponents, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _activatorComponents, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _methodComponents, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberComponents, collection, component, metadata);
        }

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < _delegateComponents.Length; i++)
            {
                var value = _delegateComponents[i]?.CanCreateDelegate(delegateType, method);
                if (value != null && value.Value)
                    return true;
            }

            return false;
        }

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < _delegateComponents.Length; i++)
            {
                var value = _delegateComponents[i]?.TryCreateDelegate(delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            for (var i = 0; i < _activatorComponents.Length; i++)
            {
                var value = _activatorComponents[i]?.TryGetActivator(constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < _methodComponents.Length; i++)
            {
                var value = _methodComponents[i]?.TryGetMethodInvoker(method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Delegate GetMethodInvoker(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < _methodComponents.Length; i++)
            {
                var value = _methodComponents[i]?.TryGetMethodInvoker(delegateType, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            for (var i = 0; i < _memberComponents.Length; i++)
            {
                var value = _memberComponents[i]?.TryGetMemberGetter<TType>(member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Action<object?, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            for (var i = 0; i < _memberComponents.Length; i++)
            {
                var value = _memberComponents[i]?.TryGetMemberSetter<TType>(member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        #endregion

        #region Methods

        private void ThrowNotInitialized()
        {
            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IReflectionDelegateProviderComponent).Name);
        }

        #endregion
    }
}