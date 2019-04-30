using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;

// ReSharper disable FieldCanBeMadeReadOnly.Local
namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class ReflectionDelegateProvider : IReflectionDelegateProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IReflectionDelegateFactory>? _reflectionDelegateFactories;

        private static readonly Dictionary<ConstructorInfo, Func<object?[], object>> ActivatorCache;
        private static readonly Dictionary<MethodInfo, Func<object?, object?[], object?>> InvokeMethodCache;
        private static readonly Dictionary<MethodDelegateCacheKey, Delegate> InvokeMethodCacheDelegate;
        private static readonly Dictionary<MemberInfoDelegateCacheKey, Delegate> MemberGetterCache;
        private static readonly Dictionary<MemberInfoDelegateCacheKey, Delegate> MemberSetterCache;

        #endregion

        #region Constructors

        static ReflectionDelegateProvider()
        {
            ActivatorCache = new Dictionary<ConstructorInfo, Func<object?[], object>>(MemberInfoEqualityComparer.Instance);
            InvokeMethodCache = new Dictionary<MethodInfo, Func<object?, object?[], object?>>(MemberInfoEqualityComparer.Instance);
            InvokeMethodCacheDelegate = new Dictionary<MethodDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            MemberGetterCache = new Dictionary<MemberInfoDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
            MemberSetterCache = new Dictionary<MemberInfoDelegateCacheKey, Delegate>(MemberCacheKeyComparer.Instance);
        }

        [Preserve(Conditional = true)]
        public ReflectionDelegateProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IReflectionDelegateFactory> ReflectionDelegateFactories
        {
            get
            {
                if (_reflectionDelegateFactories == null)
                    _componentCollectionProvider.LazyInitialize(ref _reflectionDelegateFactories, this);
                return _reflectionDelegateFactories;
            }
        }

        #endregion

        #region Implementation of interfaces

        public Func<object[], object> GetActivatorDelegate(ConstructorInfo constructor)
        {
            Should.NotBeNull(constructor, nameof(constructor));
            lock (ActivatorCache)
            {
                if (!ActivatorCache.TryGetValue(constructor, out var value))
                {
                    var items = ReflectionDelegateFactories.GetItems();
                    for (var i = 0; i < items.Length; i++)
                    {
                        value = items[i].TryGetActivatorDelegate(this, constructor);
                        if (value != null)
                            break;
                    }

                    ThrowNotInitializedIfNeed(value);
                    ActivatorCache[constructor] = value;
                }

                return value;
            }
        }

        public Func<object, object[], object> GetMethodDelegate(MethodInfo method)
        {
            Should.NotBeNull(method, nameof(method));
            lock (InvokeMethodCache)
            {
                if (!InvokeMethodCache.TryGetValue(method, out var value))
                {
                    var items = ReflectionDelegateFactories.GetItems();
                    for (var i = 0; i < items.Length; i++)
                    {
                        value = items[i].TryGetMethodDelegate(this, method);
                        if (value != null)
                            break;
                    }

                    ThrowNotInitializedIfNeed(value);
                    InvokeMethodCache[method] = value;
                }

                return value;
            }
        }

        public Delegate GetMethodDelegate(Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            var cacheKey = new MethodDelegateCacheKey(method, delegateType);
            lock (InvokeMethodCacheDelegate)
            {
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    var items = ReflectionDelegateFactories.GetItems();
                    for (var i = 0; i < items.Length; i++)
                    {
                        value = items[i].TryGetMethodDelegate(this, delegateType, method);
                        if (value != null)
                            break;
                    }

                    ThrowNotInitializedIfNeed(value);
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        public Func<object, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberGetterCache)
            {
                if (!MemberGetterCache.TryGetValue(key, out var value))
                {
                    var items = ReflectionDelegateFactories.GetItems();
                    for (var i = 0; i < items.Length; i++)
                    {
                        value = items[i].TryGetMemberGetter<TType>(this, member);
                        if (value != null)
                            break;
                    }

                    ThrowNotInitializedIfNeed(value);
                    MemberGetterCache[key] = value;
                }

                return (Func<object?, TType>) value;
            }
        }

        public Action<object, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberSetterCache)
            {
                if (!MemberSetterCache.TryGetValue(key, out var value))
                {
                    var items = ReflectionDelegateFactories.GetItems();
                    for (var i = 0; i < items.Length; i++)
                    {
                        value = items[i].TryGetMemberSetter<TType>(this, member);
                        if (value != null)
                            break;
                    }

                    ThrowNotInitializedIfNeed(value);
                    MemberSetterCache[key] = value;
                }

                return (Action<object?, TType>) value;
            }
        }

        #endregion

        #region Methods

        private void ThrowNotInitializedIfNeed(Delegate? result)
        {
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IReflectionDelegateFactory).Name);
        }

        #endregion

        #region Nested types

        private sealed class MemberCacheKeyComparer : IEqualityComparer<MethodDelegateCacheKey>, IEqualityComparer<MemberInfoDelegateCacheKey>
        {
            #region Fields

            public static readonly MemberCacheKeyComparer Instance;

            #endregion

            #region Constructors

            static MemberCacheKeyComparer()
            {
                Instance = new MemberCacheKeyComparer();
            }

            private MemberCacheKeyComparer()
            {
            }

            #endregion

            #region Implementation of interfaces

            bool IEqualityComparer<MemberInfoDelegateCacheKey>.Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType.EqualsEx(y.DelegateType) && x.Member.EqualsEx(y.Member);
            }

            int IEqualityComparer<MemberInfoDelegateCacheKey>.GetHashCode(MemberInfoDelegateCacheKey obj)
            {
                unchecked
                {
                    return obj.DelegateType.GetHashCode() * 397 ^ obj.Member.GetHashCode();
                }
            }

            bool IEqualityComparer<MethodDelegateCacheKey>.Equals(MethodDelegateCacheKey x, MethodDelegateCacheKey y)
            {
                return x.DelegateType.EqualsEx(y.DelegateType) && x.Method.EqualsEx(y.Method);
            }

            int IEqualityComparer<MethodDelegateCacheKey>.GetHashCode(MethodDelegateCacheKey obj)
            {
                unchecked
                {
                    return obj.DelegateType.GetHashCode() * 397 ^ obj.Method.GetHashCode();
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct MethodDelegateCacheKey
        {
            #region Fields

            public MethodInfo Method;
            public Type DelegateType;

            #endregion

            #region Constructors

            public MethodDelegateCacheKey(MethodInfo method, Type delegateType)
            {
                Method = method;
                DelegateType = delegateType;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct MemberInfoDelegateCacheKey
        {
            #region Fields

            public MemberInfo Member;
            public Type DelegateType;

            #endregion

            #region Constructors

            public MemberInfoDelegateCacheKey(MemberInfo member, Type delegateType)
            {
                Member = member;
                DelegateType = delegateType;
            }

            #endregion
        }

        #endregion
    }
}