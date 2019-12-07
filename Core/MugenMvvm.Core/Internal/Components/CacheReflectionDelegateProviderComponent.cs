﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class CacheReflectionDelegateProviderComponent : DecoratorComponentBase<IReflectionDelegateProvider, IActivatorReflectionDelegateProviderComponent>,
        IActivatorReflectionDelegateProviderComponent, IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent,
        IDecoratorComponentCollectionComponent<IMemberReflectionDelegateProviderComponent>, IDecoratorComponentCollectionComponent<IMethodReflectionDelegateProviderComponent>, IHasPriority, IHasCache
    {
        #region Fields

        private readonly MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>?> _activatorCache;
        private readonly MemberInfoDelegateCache<Delegate?> _activatorCacheDelegate;
        private readonly MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>?> _invokeMethodCache;
        private readonly MemberInfoDelegateCache<Delegate?> _invokeMethodCacheDelegate;
        private readonly MemberInfoDelegateCache<Delegate?> _memberGetterCache;
        private readonly MemberInfoDelegateCache<Delegate?> _memberSetterCache;

        private IMemberReflectionDelegateProviderComponent[] _memberComponents;
        private IMethodReflectionDelegateProviderComponent[] _methodComponents;

        #endregion

        #region Constructors

        public CacheReflectionDelegateProviderComponent()
        {
            _memberComponents = Default.EmptyArray<IMemberReflectionDelegateProviderComponent>();
            _methodComponents = Default.EmptyArray<IMethodReflectionDelegateProviderComponent>();
            _activatorCache = new MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>?>(59);
            _activatorCacheDelegate = new MemberInfoDelegateCache<Delegate?>();
            _invokeMethodCache = new MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>?>(59);
            _invokeMethodCacheDelegate = new MemberInfoDelegateCache<Delegate?>();
            _memberGetterCache = new MemberInfoDelegateCache<Delegate?>();
            _memberSetterCache = new MemberInfoDelegateCache<Delegate?>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public Func<object?[], object>? TryGetActivator(ConstructorInfo constructor)
        {
            lock (_activatorCache)
            {
                if (!_activatorCache.TryGetValue(constructor, out var value))
                {
                    value = TryGetActivatorInternal(constructor);
                    _activatorCache[constructor] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            var cacheKey = new MemberInfoDelegateCacheKey(constructor, delegateType);
            lock (_activatorCacheDelegate)
            {
                if (!_activatorCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = TryGetActivatorInternal(constructor, delegateType);
                    _activatorCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        void IDecoratorComponentCollectionComponent<IMemberReflectionDelegateProviderComponent>.Decorate(IList<IMemberReflectionDelegateProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorDecorate(this, Owner, components, this, ref _memberComponents);
            Invalidate(false, false, true);
        }

        void IDecoratorComponentCollectionComponent<IMethodReflectionDelegateProviderComponent>.Decorate(IList<IMethodReflectionDelegateProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorDecorate(this, Owner, components, this, ref _methodComponents);
            Invalidate(false, true, false);
        }

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Invalidate(true, true, true);
        }

        public Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            var key = new MemberInfoDelegateCacheKey(member, delegateType);
            lock (_memberGetterCache)
            {
                if (!_memberGetterCache.TryGetValue(key, out var value))
                {
                    value = TryGetMemberGetterInternal(member, delegateType);
                    _memberGetterCache[key] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            var key = new MemberInfoDelegateCacheKey(member, delegateType);
            lock (_memberSetterCache)
            {
                if (!_memberSetterCache.TryGetValue(key, out var value))
                {
                    value = TryGetMemberSetterInternal(member, delegateType);
                    _memberSetterCache[key] = value;
                }

                return value;
            }
        }

        public Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method)
        {
            lock (_invokeMethodCache)
            {
                if (!_invokeMethodCache.TryGetValue(method, out var value))
                {
                    value = TryGetMethodInvokerInternal(method);
                    _invokeMethodCache[method] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            var cacheKey = new MemberInfoDelegateCacheKey(method, delegateType);
            lock (_invokeMethodCacheDelegate)
            {
                if (!_invokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = TryGetMethodInvokerInternal(method, delegateType);
                    _invokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        #endregion

        #region Methods

        protected override void Decorate(IList<IActivatorReflectionDelegateProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.Decorate(components, metadata);
            Invalidate(true, false, false);
        }

        private Func<object?[], object>? TryGetActivatorInternal(ConstructorInfo constructor)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(constructor);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        private Delegate? TryGetActivatorInternal(ConstructorInfo constructor, Type delegateType)
        {
            var components = Components;
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(constructor, delegateType);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        private Delegate? TryGetMemberGetterInternal(MemberInfo member, Type delegateType)
        {
            var components = _memberComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var getter = components[i].TryGetMemberGetter(member, delegateType);
                if (getter != null)
                    return getter;
            }

            return null;
        }

        private Delegate? TryGetMemberSetterInternal(MemberInfo member, Type delegateType)
        {
            var components = _memberComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var setter = components[i].TryGetMemberSetter(member, delegateType);
                if (setter != null)
                    return setter;
            }

            return null;
        }

        private Func<object?, object?[], object?>? TryGetMethodInvokerInternal(MethodInfo method)
        {
            var components = _methodComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(method);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        private Delegate? TryGetMethodInvokerInternal(MethodInfo method, Type delegateType)
        {
            var components = _methodComponents;
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(method, delegateType);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        private void Invalidate(bool activator, bool method, bool member)
        {
            if (activator)
            {
                lock (_activatorCacheDelegate)
                {
                    _activatorCacheDelegate.Clear();
                }

                lock (_activatorCache)
                {
                    _activatorCache.Clear();
                }
            }

            if (method)
            {
                lock (_invokeMethodCache)
                {
                    _invokeMethodCache.Clear();
                }

                lock (_invokeMethodCacheDelegate)
                {
                    _invokeMethodCacheDelegate.Clear();
                }
            }

            if (member)
            {
                lock (_memberSetterCache)
                {
                    _memberSetterCache.Clear();
                }

                lock (_memberGetterCache)
                {
                    _memberGetterCache.Clear();
                }
            }
        }

        #endregion

        #region Nested types

        private sealed class MemberInfoDelegateCache<TValue> : LightDictionary<MemberInfoDelegateCacheKey, TValue>
        {
            #region Constructors

            public MemberInfoDelegateCache() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType == y.DelegateType && x.Member == y.Member;
            }

            protected override int GetHashCode(MemberInfoDelegateCacheKey key)
            {
                return HashCode.Combine(key.DelegateType, key.Member);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MemberInfoDelegateCacheKey
        {
            #region Fields

            public readonly MemberInfo Member;
            public readonly Type DelegateType;

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