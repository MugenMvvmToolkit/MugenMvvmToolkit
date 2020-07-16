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
            return GetComponentOptional(item)?.GetValues(this, item, predicate, state) ?? default;
        }

        public bool TryGet(object item, string path, out object? value)
        {
            var component = GetComponentOptional(item);
            if (component == null)
            {
                value = default!;
                return false;
            }

            return component.TryGet(this, item, path, out value!);
        }

        public bool Contains(object item, string path)
        {
            var component = GetComponentOptional(item);
            return component != null && component.Contains(this, item, path);
        }

        public object? AddOrUpdate(object item, string path, object? addValue, UpdateValueDelegate<object, object?, object?, object?, object?> updateValueFactory, object? state = null)
        {
            return GetComponent(item).AddOrUpdate(this, item, path, addValue, updateValueFactory, state);
        }

        public object? AddOrUpdate(object item, string path, Func<object, object?, object?> addValueFactory, UpdateValueDelegate<object, object?, object?, object?> updateValueFactory, object? state = null)
        {
            return GetComponent(item).AddOrUpdate(this, item, path, addValueFactory, updateValueFactory, state);
        }

        public object? GetOrAdd(object item, string path, Func<object, object?, object?> valueFactory, object? state = null)
        {
            return GetComponent(item).GetOrAdd(this, item, path, valueFactory, state);
        }

        public object? GetOrAdd(object item, string path, object? value)
        {
            return GetComponent(item).GetOrAdd(this, item, path, value);
        }

        public void Set(object item, string path, object? value, out object? oldValue)
        {
            GetComponent(item).Set(this, item, path, value, out oldValue);
        }

        public bool Clear(object item, string path, out object? oldValue)
        {
            return GetComponent(item).Clear(this, item, path, out oldValue);
        }

        public bool Clear(object item)
        {
            return GetComponent(item).Clear(this, item);
        }

        #endregion

        #region Methods

        private IAttachedValueProviderComponent? GetComponentOptional(object item)
        {
            if (_components == null)
                _componentTracker.Attach(this);
            return _components!.TryGetProvider(this, item, null);
        }

        private IAttachedValueProviderComponent GetComponent(object item)
        {
            if (_components == null)
                _componentTracker.Attach(this);
            var provider = _components!.TryGetProvider(this, item, null);
            if (provider == null)
                ExceptionManager.ThrowObjectNotInitialized(this, _components);
            return provider;
        }

        #endregion
    }
}