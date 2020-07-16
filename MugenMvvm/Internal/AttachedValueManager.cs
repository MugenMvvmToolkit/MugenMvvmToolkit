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

        public ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(object item, Func<object, KeyValuePair<string, object?>, object?, bool>? predicate, object? state)
        {
            return GetComponentOptional(item)?.GetValues(item, predicate, state) ?? default;
        }

        public bool TryGet(object item, string path, out object? value)
        {
            var component = GetComponentOptional(item);
            if (component == null)
            {
                value = default!;
                return false;
            }

            return component.TryGet(item, path, out value!);
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

        public TValue GetOrAdd<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> valueFactory) where TItem : class
        {
            return GetComponent(item).GetOrAdd(item, path, state, valueFactory);
        }

        public object? GetOrAdd(object item, string path, object? value)
        {
            return GetComponent(item).GetOrAdd(item, path, value);
        }

        public void Set(object item, string path, object? value, out object? oldValue)
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