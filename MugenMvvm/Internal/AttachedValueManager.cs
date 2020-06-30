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
    public sealed class AttachedValueManager : ComponentOwnerBase<IAttachedValueManager>, IAttachedValueManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IAttachedValueProviderComponent?[]? _components;

        #endregion

        #region Constructors

        public AttachedValueManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IAttachedValueProviderComponent, AttachedValueManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues<TItem, TState>(TItem item, in TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate = null) where TItem : class
        {
            return GetComponentOptional(item)?.TryGetValues(item, state, predicate) ?? default;
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

        public TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, in TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            return GetComponent(item).AddOrUpdate(item, path, addValue, state, updateValueFactory);
        }

        public TValue AddOrUpdate<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> addValueFactory, UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory) where TItem : class
        {
            return GetComponent(item).AddOrUpdate(item, path, state, addValueFactory, updateValueFactory);
        }

        public TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            return GetComponent(item).GetOrAdd(item, path, value);
        }

        public TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> valueFactory) where TItem : class
        {
            return GetComponent(item).GetOrAdd(item, path, state, valueFactory);
        }

        public void Set<TValue>(object item, string path, TValue value, out object? oldValue)
        {
            GetComponent(item).Set(item, path, value, out oldValue);
        }

        public bool Clear(object item, string path, out object? oldValue)
        {
            return GetComponent(item).Clear(item, path, out oldValue);
        }

        public bool Clear(object item)
        {
            return GetComponent(item).Clear(item);
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