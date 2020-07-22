using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueStorageProviderBase : IAttachedValueStorageProviderComponent, IAttachedValueStorageManager
    {
        #region Implementation of interfaces

        public AttachedValueStorage TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            if (IsSupported(attachedValueManager, item, metadata))
                return new AttachedValueStorage(item, this, GetAttachedDictionary(item, true));
            return default;
        }

        public int GetCount(object item, ref object? internalState)
        {
            if (internalState == null)
                return 0;
            return ((IDictionary<string, object?>)internalState).Count;
        }

        public ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues<TState>(object item, ref object? internalState, TState state = default,
            Func<object, KeyValuePair<string, object?>, TState, bool>? predicate = null)
        {
            if (internalState == null)
                return default;
            var dictionary = (IDictionary<string, object?>)internalState;
            lock (dictionary)
            {
                if (dictionary.Count == 0)
                    return default;

                if (predicate == null)
                {
                    if (dictionary.Count == 1)
                        return dictionary.FirstOrDefault();
                    return ItemOrList.FromListToReadOnly(new List<KeyValuePair<string, object?>>(dictionary));
                }

                var result = ItemOrListEditor.Get<KeyValuePair<string, object?>>(pair => pair.Key == null);
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue, state))
                        result.Add(keyValue);
                }

                return result.ToItemOrList<IReadOnlyList<KeyValuePair<string, object?>>>();
            }
        }

        public bool Contains(object item, ref object? internalState, string path)
        {
            if (internalState == null)
                return false;
            lock (internalState)
            {
                return ((IDictionary<string, object?>)internalState).ContainsKey(path);
            }
        }

        public bool TryGet(object item, ref object? internalState, string path, out object? value)
        {
            if (internalState == null)
            {
                value = null;
                return false;
            }

            lock (internalState)
            {
                return ((IDictionary<string, object?>)internalState).TryGetValue(path, out value);
            }
        }

        public TValue AddOrUpdate<TValue, TState>(object item, ref object? internalState, string path, TValue addValue, TState state, UpdateValueDelegate<object, TValue, TValue, TState, TValue> updateValueFactory)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValue, (TValue)value!, state));
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public TValue AddOrUpdate<TValue, TState>(object item, ref object? internalState, string path, TState state, Func<object, TState, TValue> addValueFactory,
            UpdateValueDelegate<object, TValue, TState, TValue> updateValueFactory)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, addValueFactory, (TValue)value!, state));
                    dictionary[path] = value;
                    return (TValue)value!;
                }

                value = BoxingExtensions.Box(addValueFactory(item, state));
                dictionary.Add(path, value);
                return (TValue)value!;
            }
        }

        public TValue GetOrAdd<TValue, TState>(object item, ref object? internalState, string path, TState state, Func<object, TState, TValue> valueFactory)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue!;
                oldValue = BoxingExtensions.Box(valueFactory(item, state));
                dictionary.Add(path, oldValue);
                return (TValue)oldValue!;
            }
        }

        public TValue GetOrAdd<TValue>(object item, ref object? internalState, string path, TValue value)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue!;
                dictionary.Add(path, BoxingExtensions.Box(value));
                return value;
            }
        }

        public void Set(object item, ref object? internalState, string path, object? value, out object? oldValue)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                dictionary.TryGetValue(path, out oldValue);
                dictionary[path] = value;
            }
        }

        public bool Remove(object item, ref object? internalState, string path, out object? oldValue)
        {
            if (internalState == null)
            {
                oldValue = null;
                return false;
            }

            var dictionary = (IDictionary<string, object?>)internalState;
            lock (dictionary)
            {
                return dictionary.TryGetValue(path, out oldValue) && dictionary.Remove(path);
            }
        }

        public bool Clear(object item, ref object? internalState)
        {
            if (internalState == null)
                return false;
            internalState = null;
            return ClearInternal(item);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDictionary<string, object?> GetDictionary(object item, ref object? internalState)
        {
            internalState ??= GetAttachedDictionary(item, false)!;
            return (IDictionary<string, object?>)internalState;
        }

        protected abstract bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata);

        protected abstract IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional);

        protected abstract bool ClearInternal(object item);

        #endregion
    }
}