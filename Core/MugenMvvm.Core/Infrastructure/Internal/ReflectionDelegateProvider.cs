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

        public Func<object[], object> GetActivatorDelegate(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetActivatorDelegate(this, constructor);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Func<object, object[], object> GetMethodDelegate(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMethodDelegate(this, method);
                if (value != null)
                    return value;
            }

            ThrowNotInitialized();
            return null;
        }

        public Delegate GetMethodDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].TryGetMethodDelegate(this, delegateType, method);
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