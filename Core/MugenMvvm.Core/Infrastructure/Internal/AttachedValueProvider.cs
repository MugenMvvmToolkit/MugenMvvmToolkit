using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class AttachedValueProvider : IAttachedValueProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IChildAttachedValueProvider>? _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedValueProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildAttachedValueProvider> Providers
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

        public IReadOnlyList<KeyValuePair<string, object?>> GetValues<TItem>(TItem item, Func<TItem, string, object?, bool>? predicate)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();
            return dictionary.GetValues(item, predicate);
        }

        public bool TryGetValue<TItem, TValue>(TItem item, string path, out TValue value)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary != null)
                return dictionary.TryGetValue(item, path, out value);
            value = default!;
            return false;
        }

        public bool Contains<TItem>(TItem item, string path)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            return dictionary != null && dictionary.Contains(item, path);
        }

        public TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TValue addValue, TState1 state1, TState2 state2,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            return GetOrAddAttachedDictionary(item, true)!.AddOrUpdate(item, path, addValue, state1, state2, updateValueFactory);
        }

        public TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2,
            Func<TItem, TState1, TState2, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            return GetOrAddAttachedDictionary(item, true)!.AddOrUpdate(item, path, state1, state2, addValueFactory, updateValueFactory);
        }

        public TValue GetOrAdd<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            return GetOrAddAttachedDictionary(item, true)!.GetOrAdd(item, path, value);
        }

        public TValue GetOrAdd<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            return GetOrAddAttachedDictionary(item, true)!.GetOrAdd(item, path, state1, state2, valueFactory);
        }

        public void SetValue<TItem, TValue>(TItem item, string path, TValue value)
            where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            GetOrAddAttachedDictionary(item, true)!.SetValue(item, path, value);
        }

        public bool Clear<TItem>(TItem item, string path) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            return dictionary.Clear(item, path);
        }

        public bool Clear<TItem>(TItem item) where TItem : class
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            return dictionary.Clear(item);
        }

        #endregion

        #region Methods

        private IAttachedValueProviderDictionary? GetOrAddAttachedDictionary(object item, bool required)
        {
            var items = _providers.GetItemsOrDefault();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].TryGetOrAddAttachedDictionary(this, item, required, out var dict))
                    return dict;
            }

            if (required)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return null;
        }

        #endregion
    }
}