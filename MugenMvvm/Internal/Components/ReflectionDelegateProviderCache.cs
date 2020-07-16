using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ReflectionDelegateProviderCache : ComponentCacheBase<IReflectionManager, IActivatorReflectionDelegateProviderComponent>,
        IActivatorReflectionDelegateProviderComponent, IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent,
        IComponentCollectionDecorator<IMemberReflectionDelegateProviderComponent>, IComponentCollectionDecorator<IMethodReflectionDelegateProviderComponent>, IHasPriority
    {
        #region Fields

        private readonly Dictionary<ConstructorInfo, Func<object?[], object>?> _activatorCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _activatorCacheDelegate;
        private readonly Dictionary<MethodInfo, Func<object?, object?[], object?>?> _invokeMethodCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _invokeMethodCacheDelegate;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _memberGetterCache;
        private readonly Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?> _memberSetterCache;

        private IMemberReflectionDelegateProviderComponent[] _memberComponents;
        private IMethodReflectionDelegateProviderComponent[] _methodComponents;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionDelegateProviderCache()
        {
            _memberComponents = Default.Array<IMemberReflectionDelegateProviderComponent>();
            _methodComponents = Default.Array<IMethodReflectionDelegateProviderComponent>();
            _activatorCache = new Dictionary<ConstructorInfo, Func<object?[], object>?>(59, InternalComparer.MemberInfo);
            _activatorCacheDelegate = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalComparer.TypeMember);
            _invokeMethodCache = new Dictionary<MethodInfo, Func<object?, object?[], object?>?>(59, InternalComparer.MemberInfo);
            _invokeMethodCacheDelegate = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalComparer.TypeMember);
            _memberGetterCache = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalComparer.TypeMember);
            _memberSetterCache = new Dictionary<KeyValuePair<Type, MemberInfo>, Delegate?>(23, InternalComparer.TypeMember);
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
                    value = Components.TryGetActivator(constructor);
                    _activatorCache[constructor] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            var cacheKey = new KeyValuePair<Type, MemberInfo>(delegateType, constructor);
            lock (_activatorCacheDelegate)
            {
                if (!_activatorCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = Components.TryGetActivator(constructor, delegateType);
                    _activatorCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        void IComponentCollectionDecorator<IMemberReflectionDelegateProviderComponent>.Decorate(IComponentCollection collection, IList<IMemberReflectionDelegateProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            _memberComponents = this.Decorate(components);
        }

        void IComponentCollectionDecorator<IMethodReflectionDelegateProviderComponent>.Decorate(IComponentCollection collection, IList<IMethodReflectionDelegateProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            _methodComponents = this.Decorate(components);
        }

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Invalidate(true, true, true);
        }

        public Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            var key = new KeyValuePair<Type, MemberInfo>(delegateType, member);
            lock (_memberGetterCache)
            {
                if (!_memberGetterCache.TryGetValue(key, out var value))
                {
                    value = _memberComponents.TryGetMemberGetter(member, delegateType);
                    _memberGetterCache[key] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            var key = new KeyValuePair<Type, MemberInfo>(delegateType, member);
            lock (_memberSetterCache)
            {
                if (!_memberSetterCache.TryGetValue(key, out var value))
                {
                    value = _memberComponents.TryGetMemberSetter(member, delegateType);
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
                    value = _methodComponents.TryGetMethodInvoker(method);
                    _invokeMethodCache[method] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            var cacheKey = new KeyValuePair<Type, MemberInfo>(delegateType, method);
            lock (_invokeMethodCacheDelegate)
            {
                if (!_invokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = _methodComponents.TryGetMethodInvoker(method, delegateType);
                    _invokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        #endregion

        #region Methods

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            InvalidateComponent(component);
        }

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            InvalidateComponent(component);
        }

        private void InvalidateComponent(object component)
        {
            Invalidate(component is IActivatorReflectionDelegateProviderComponent, component is IMethodReflectionDelegateProviderComponent, component is IMemberReflectionDelegateProviderComponent);
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
    }
}