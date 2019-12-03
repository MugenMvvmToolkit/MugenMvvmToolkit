using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;

namespace MugenMvvm.Internal
{
    public sealed class ReflectionDelegateProvider : ComponentOwnerBase<IReflectionDelegateProvider>, IReflectionDelegateProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionDelegateProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var components = GetComponents<IReflectionDelegateProviderComponent>(null);
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
            var components = GetComponents<IReflectionDelegateProviderComponent>(null);
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
            var components = GetComponents<IActivatorReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetActivator(constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IActivatorReflectionDelegateProviderComponent));
            return null;
        }

        public Delegate GetActivator(ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            Should.NotBeNull(delegateType, nameof(delegateType));
            var components = GetComponents<IActivatorReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetActivator(constructor, delegateType);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IActivatorReflectionDelegateProviderComponent));
            return null;
        }

        public Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            var components = GetComponents<IMethodReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMethodInvoker(method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IMethodReflectionDelegateProviderComponent));
            return null;
        }

        public Delegate GetMethodInvoker(MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var components = GetComponents<IMethodReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMethodInvoker(method, delegateType);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IMethodReflectionDelegateProviderComponent));
            return null;
        }

        public Delegate GetMemberGetter(MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(member, nameof(member));
            var components = GetComponents<IMemberReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMemberGetter(member, delegateType);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IMemberReflectionDelegateProviderComponent));
            return null;
        }

        public Delegate GetMemberSetter(MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(member, nameof(member));
            var components = GetComponents<IMemberReflectionDelegateProviderComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryGetMemberSetter(member, delegateType);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized(typeof(IMemberReflectionDelegateProviderComponent));
            return null;
        }

        #endregion

        #region Methods

        [DoesNotReturn]
        private void ThrowNotInitialized(Type type)
        {
            ExceptionManager.ThrowObjectNotInitialized(this, type.Name);
        }

        #endregion
    }
}