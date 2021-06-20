using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class ReflectionDelegateProviderCache : ComponentCacheBase<IReflectionManager, IActivatorReflectionDelegateProviderComponent>,
        IActivatorReflectionDelegateProviderComponent, IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent,
        IComponentCollectionDecorator<IMemberReflectionDelegateProviderComponent>, IComponentCollectionDecorator<IMethodReflectionDelegateProviderComponent>
    {
        private readonly Dictionary<ConstructorInfo, Func<ItemOrArray<object?>, object>?> _activatorCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _activatorCacheDelegate;
        private readonly Dictionary<MethodInfo, Func<object?, ItemOrArray<object?>, object?>?> _invokeMethodCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _invokeMethodCacheDelegate;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _memberGetterCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _memberSetterCache;

        private ItemOrArray<IMemberReflectionDelegateProviderComponent> _memberComponents;
        private ItemOrArray<IMethodReflectionDelegateProviderComponent> _methodComponents;

        [Preserve(Conditional = true)]
        public ReflectionDelegateProviderCache(int priority = InternalComponentPriority.DelegateProviderCache) : base(priority)
        {
            _activatorCache = new Dictionary<ConstructorInfo, Func<ItemOrArray<object?>, object>?>(59, InternalEqualityComparer.MemberInfo);
            _activatorCacheDelegate = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalEqualityComparer.TypeMember);
            _invokeMethodCache = new Dictionary<MethodInfo, Func<object?, ItemOrArray<object?>, object?>?>(59, InternalEqualityComparer.MemberInfo);
            _invokeMethodCacheDelegate = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalEqualityComparer.TypeMember);
            _memberGetterCache = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalEqualityComparer.TypeMember);
            _memberSetterCache = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalEqualityComparer.TypeMember);
        }

        public Func<ItemOrArray<object?>, object>? TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            lock (_activatorCache)
            {
                if (!_activatorCache.TryGetValue(constructor, out var value))
                {
                    value = Components.TryGetActivator(reflectionManager, constructor);
                    _activatorCache[constructor] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType)
        {
            var cacheKey = new KeyValuePair<Type, MemberInfo>(delegateType, constructor);
            lock (_activatorCacheDelegate)
            {
                if (!_activatorCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = Components.TryGetActivator(reflectionManager, constructor, delegateType);
                    _activatorCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMemberGetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            var key = new KeyValuePair<Type, MemberInfo>(delegateType, member);
            lock (_memberGetterCache)
            {
                if (!_memberGetterCache.TryGetValue(key, out var value))
                {
                    value = _memberComponents.TryGetMemberGetter(reflectionManager, member, delegateType);
                    _memberGetterCache[key] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMemberSetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            var key = new KeyValuePair<Type, MemberInfo>(delegateType, member);
            lock (_memberSetterCache)
            {
                if (!_memberSetterCache.TryGetValue(key, out var value))
                {
                    value = _memberComponents.TryGetMemberSetter(reflectionManager, member, delegateType);
                    _memberSetterCache[key] = value;
                }

                return value;
            }
        }

        public Func<object?, ItemOrArray<object?>, object?>? TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method)
        {
            lock (_invokeMethodCache)
            {
                if (!_invokeMethodCache.TryGetValue(method, out var value))
                {
                    value = _methodComponents.TryGetMethodInvoker(reflectionManager, method);
                    _invokeMethodCache[method] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType)
        {
            var cacheKey = new KeyValuePair<Type, MemberInfo>(delegateType, method);
            lock (_invokeMethodCacheDelegate)
            {
                if (!_invokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = _methodComponents.TryGetMethodInvoker(reflectionManager, method, delegateType);
                    _invokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata) => Invalidate(true, true, true);

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => InvalidateComponent(component);

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => InvalidateComponent(component);

        private void InvalidateComponent(object component) => Invalidate(component is IActivatorReflectionDelegateProviderComponent,
            component is IMethodReflectionDelegateProviderComponent,
            component is IMemberReflectionDelegateProviderComponent);

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

        void IComponentCollectionDecorator<IMemberReflectionDelegateProviderComponent>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IMemberReflectionDelegateProviderComponent> components,
            IReadOnlyMetadataContext? metadata) => _memberComponents = this.Decorate(ref components);

        void IComponentCollectionDecorator<IMethodReflectionDelegateProviderComponent>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IMethodReflectionDelegateProviderComponent> components,
            IReadOnlyMetadataContext? metadata) => _methodComponents = this.Decorate(ref components);
    }
}