using System;
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
        public ReflectionDelegateProvider(IComponentCollectionProvider componentCollectionProvider) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.CanCreateDelegate(delegateType, method);
                if (value != null && value.Value)
                    return true;
            }

            return false;
        }

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryCreateDelegate(delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryGetActivator(constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryGetMethodInvoker(method);
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
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryGetMethodInvoker(delegateType, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Func<object?, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryGetMemberGetter<TType>(member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null!;
        }

        public Action<object?, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = (items[i] as IReflectionDelegateProviderComponent)?.TryGetMemberSetter<TType>(member);
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