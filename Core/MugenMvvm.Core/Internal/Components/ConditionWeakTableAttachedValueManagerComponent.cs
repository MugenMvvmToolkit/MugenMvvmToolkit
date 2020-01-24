using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ConditionWeakTableAttachedValueManagerComponent : IAttachedValueManagerComponent, IAttachedValueProvider, IHasPriority
    {
        #region Fields

        private readonly ConditionalWeakTable<object, StringOrdinalLightDictionary<object>> _weakTable;

        #endregion

        #region Constructors

        public ConditionWeakTableAttachedValueManagerComponent()
        {
            _weakTable = new ConditionalWeakTable<object, StringOrdinalLightDictionary<object>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.AttachedValueManager;

        #endregion

        #region Implementation of interfaces

        public bool TryGetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            provider = _weakTable.TryGetValue(item, out _) ? this : null;
            return true;
        }

        public IAttachedValueProvider? TryGetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        IReadOnlyList<KeyValuePair<string, object?>> IAttachedValueProvider.GetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate)
        {
            _weakTable.TryGetValue(item, out var dictionary);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();

            lock (dictionary)
            {
                return dictionary.GetValues(item, state, predicate);
            }
        }

        bool IAttachedValueProvider.TryGet<TValue>(object item, string path, out TValue value)
        {
            _weakTable.TryGetValue(item, out var dictionary);
            if (dictionary == null)
            {
                value = default;
                return false;
            }

            lock (dictionary)
            {
                return dictionary.TryGetValue(path, out value);
            }
        }

        bool IAttachedValueProvider.Contains(object item, string path)
        {
            _weakTable.TryGetValue(item, out var dictionary);
            if (dictionary == null)
                return false;

            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        TValue IAttachedValueProvider.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory)
        {
            var dictionary = GetDictionary(item);
            lock (dictionary)
            {
                return dictionary.AddOrUpdate(item, path, addValue, state, updateValueFactory);
            }
        }

        TValue IAttachedValueProvider.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory)
        {
            var dictionary = GetDictionary(item);
            lock (dictionary)
            {
                return dictionary.AddOrUpdate(item, path, state, addValueFactory, updateValueFactory);
            }
        }

        TValue IAttachedValueProvider.GetOrAdd<TValue>(object item, string path, TValue value)
        {
            var dictionary = GetDictionary(item);
            lock (dictionary)
            {
                return dictionary.GetOrAdd(path, value);
            }
        }

        TValue IAttachedValueProvider.GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
        {
            var dictionary = GetDictionary(item);
            lock (dictionary)
            {
                return dictionary.GetOrAdd(item, path, state, valueFactory);
            }
        }

        void IAttachedValueProvider.Set<TValue>(object item, string path, TValue value)
        {
            var dictionary = GetDictionary(item);
            lock (dictionary)
            {
                dictionary[path] = value;
            }
        }

        bool IAttachedValueProvider.Clear(object item, string? path)
        {
            _weakTable.TryGetValue(item, out var dictionary);
            if (dictionary == null)
                return false;

            var clear = false;
            var result = false;
            lock (dictionary)
            {
                if (string.IsNullOrEmpty(path))
                {
                    dictionary.Clear();
                    clear = true;
                    result = true;
                }
                else
                {
                    result = dictionary.Remove(path);
                    if (result && dictionary.Count == 0)
                        clear = true;
                }
            }

            if (clear)
                _weakTable.Remove(item);
            return result;
        }

        #endregion

        #region Methods

        private StringOrdinalLightDictionary<object> GetDictionary(object item)
        {
            return _weakTable.GetValue(item, key => new StringOrdinalLightDictionary<object>(3));
        }

        #endregion
    }
}