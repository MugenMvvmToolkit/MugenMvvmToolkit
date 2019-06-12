using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class ReflectionDelegateProvider : IReflectionDelegateProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IChildReflectionDelegateProvider>? _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionDelegateProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildReflectionDelegateProvider> Providers
        {
            get
            {
                if (_providers == null)
                    _componentCollectionProvider.LazyInitialize(ref _providers, this);
                return _providers;
            }
        }

        #endregion

        #region Implementation of interfaces

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].CanCreateDelegate(this, delegateType, method);
                if (value)
                    return true;
            }

            return false;
        }

        public Delegate TryCreateDelegate(Type delegateType, object target, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryCreateDelegate(this, delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public Func<object[], object> GetActivator(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetActivator(this, constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Func<object, object[], object> GetMethodInvoker(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMethodInvoker(this, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Delegate GetMethodInvoker(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMethodInvoker(this, delegateType, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Func<object, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMemberGetter<TType>(this, member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Action<object, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMemberSetter<TType>(this, member);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        #endregion

        #region Methods

        private void ThrowNotInitialized()
        {
            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IChildReflectionDelegateProvider).Name);
        }

        #endregion
    }
}