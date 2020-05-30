using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;

namespace MugenMvvm.Internal
{
    public sealed class AttachedValueProvider : ComponentOwnerBase<IAttachedValueProvider>, IAttachedValueProvider
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IAttachedValueProviderComponent?[]? _components;

        #endregion

        #region Constructors

        public AttachedValueProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IAttachedValueProviderComponent, AttachedValueProvider>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate = null) where TItem : class
        {
            return GetComponentOptional(item)?.TryGetValues(item, state, predicate) ?? Default.Array<KeyValuePair<string, object?>>();
        }

        public bool TryGet<TValue>(object item, string path, out TValue value)
        {
            var component = GetComponentOptional(item);
            if (component == null)
            {
                value = default!;
                return false;
            }

            return component.TryGet(item, path, out value);
        }

        public bool Contains(object item, string path)
        {
            var component = GetComponentOptional(item);
            return component != null && component.Contains(item, path);
        }

        public TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            return GetComponent(item).AddOrUpdate(item, path, addValue, state, updateValueFactory);
        }

        public TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            return GetComponent(item).AddOrUpdate(item, path, state, addValueFactory, updateValueFactory);
        }

        public TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            return GetComponent(item).GetOrAdd(item, path, value);
        }

        public TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory) where TItem : class
        {
            return GetComponent(item).GetOrAdd(item, path, state, valueFactory);
        }

        public void Set<TValue>(object item, string path, TValue value)
        {
            GetComponent(item).Set(item, path, value);
        }

        public bool Clear(object item, string? path = null)
        {
            return GetComponent(item).Clear(item, path);
        }

        #endregion

        #region Methods

        private IAttachedValueProviderComponent? GetComponentOptional(object item)
        {
            if (_components == null)
                _componentTracker.Attach(this);
            return _components!.TryGetProvider(item, null);
        }

        private IAttachedValueProviderComponent GetComponent(object item)
        {
            if (_components == null)
                _componentTracker.Attach(this);
            var provider = _components!.TryGetProvider(item, null);
            if (provider == null)
                ExceptionManager.ThrowObjectNotInitialized(this, _components);
            return provider;
        }

        #endregion
    }
}