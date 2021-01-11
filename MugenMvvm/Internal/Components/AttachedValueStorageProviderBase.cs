using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueStorageProviderBase<T> : IAttachedValueStorageProviderComponent, IAttachedValueStorageManager where T : class
    {
        #region Implementation of interfaces

        public int GetCount(object item, ref object? internalState)
        {
            if (internalState == null)
                return 0;
            return ((IDictionary<string, object?>) internalState).Count;
        }

        public ItemOrIReadOnlyList<KeyValuePair<string, object?>> GetValues<TState>(object item, TState state, Func<object, string, object?, TState, bool>? predicate, ref object? internalState)
        {
            if (internalState == null)
                return default;
            var dictionary = (IDictionary<string, object?>) internalState;
            lock (dictionary)
            {
                if (dictionary.Count == 0)
                    return default;

                if (predicate == null)
                {
                    if (dictionary.Count == 1)
                        return dictionary.FirstOrDefault();
                    return dictionary.ToArray();
                }

                var result = new ItemOrListEditor<KeyValuePair<string, object?>>();
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue.Key, keyValue.Value, state))
                        result.Add(keyValue);
                }

                return result.ToItemOrList();
            }
        }

        public bool Contains(object item, string path, ref object? internalState)
        {
            if (internalState == null)
                return false;
            lock (internalState)
            {
                return ((IDictionary<string, object?>) internalState).ContainsKey(path);
            }
        }

        public bool TryGet(object item, string path, ref object? internalState, out object? value)
        {
            if (internalState == null)
            {
                value = null;
                return false;
            }

            lock (internalState)
            {
                return ((IDictionary<string, object?>) internalState).TryGetValue(path, out value);
            }
        }

        public TValue AddOrUpdate<TValue, TState>(object item, string path, TValue addValue, TState state, Func<object, string, TValue, TState, TValue> updateValueFactory, ref object? internalState)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, path, (TValue) value!, state));
                    dictionary[path] = value;
                    return (TValue) value!;
                }

                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public TValue AddOrUpdate<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> addValueFactory,
            Func<object, string, TValue, TState, TValue> updateValueFactory, ref object? internalState)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = BoxingExtensions.Box(updateValueFactory(item, path, (TValue) value!, state));
                    dictionary[path] = value;
                    return (TValue) value!;
                }

                value = BoxingExtensions.Box(addValueFactory(item, state));
                dictionary.Add(path, value);
                return (TValue) value!;
            }
        }

        public TValue GetOrAdd<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> valueFactory, ref object? internalState)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue) oldValue!;
                oldValue = BoxingExtensions.Box(valueFactory(item, state));
                dictionary.Add(path, oldValue);
                return (TValue) oldValue!;
            }
        }

        public TValue GetOrAdd<TValue>(object item, string path, TValue value, ref object? internalState)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue) oldValue!;
                dictionary.Add(path, BoxingExtensions.Box(value));
                return value;
            }
        }

        public void Set(object item, string path, object? value, ref object? internalState, out object? oldValue)
        {
            var dictionary = GetDictionary(item, ref internalState);
            lock (dictionary)
            {
                dictionary.TryGetValue(path, out oldValue);
                dictionary[path] = value;
            }
        }

        public bool Remove(object item, string path, ref object? internalState, out object? oldValue)
        {
            if (internalState == null)
            {
                oldValue = null;
                return false;
            }

            var dictionary = (IDictionary<string, object?>) internalState;
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
            return ClearInternal((T) item);
        }

        public AttachedValueStorage TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            if (IsSupported(attachedValueManager, item, metadata))
                return new AttachedValueStorage(item, this, GetAttachedDictionary((T) item, true));
            return default;
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDictionary<string, object?> GetDictionary(object item, ref object? internalState)
        {
            internalState ??= GetAttachedDictionary((T) item, false)!;
            return (IDictionary<string, object?>) internalState;
        }

        protected abstract IDictionary<string, object?>? GetAttachedDictionary(T item, bool optional);

        protected abstract bool ClearInternal(T item);

        protected virtual bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => item is T;

        #endregion
    }
}