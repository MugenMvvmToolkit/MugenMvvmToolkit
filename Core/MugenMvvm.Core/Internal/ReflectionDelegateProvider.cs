﻿using System;
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
            MugenExtensions.ComponentTrackerOnAdded(ref _delegateComponents, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _activatorComponents, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _methodComponents, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _memberComponents, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IReflectionDelegateProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IReflectionDelegateProvider>> collection,
            IComponent<IReflectionDelegateProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _delegateComponents, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _activatorComponents, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _methodComponents, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberComponents, component);
        }

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var components = _delegateComponents;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanCreateDelegate(delegateType, method))
                    return true;
            }

            return false;
        }

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var components = _delegateComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryCreateDelegate(delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            var components = _activatorComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetActivator(constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            var components = _methodComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMethodInvoker(method);
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
            var components = _methodComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMethodInvoker(delegateType, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var components = _memberComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMemberGetter<TType>(member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Action<object?, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var components = _memberComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMemberSetter<TType>(member);
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