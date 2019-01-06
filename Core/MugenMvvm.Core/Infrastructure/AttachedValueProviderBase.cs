using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure
{
    public abstract class AttachedValueProviderBase : IAttachedValueProvider
    {
        #region Implementation of interfaces

        public virtual TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TValue addValue, TState1 state1, TState2 state2, UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValue, (TValue)value, state1, state2);
                    dictionary[path] = value;
                    return (TValue)value;
                }

                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> addValueFactory, UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValueFactory, (TValue)value, state1, state2);
                    dictionary[path] = value;
                    return (TValue)value;
                }

                value = addValueFactory(item, state1, state2);
                dictionary.Add(path, value);
                return (TValue)value;
            }
        }

        public virtual TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue;
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual TValue GetOrAdd<TItem, TValue, TState1, TState2>(TItem item, string path, TState1 state1, TState2 state2, Func<TItem, TState1, TState2, TValue> valueFactory)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            var dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return (TValue)oldValue;
                oldValue = valueFactory(item, state1, state2);
                dictionary.Add(path, oldValue);
                return (TValue)oldValue;
            }
        }

        public virtual bool TryGetValue<TValue>(object item, string path, out TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
            {
                value = default;
                return false;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var result))
                {
                    value = (TValue)result;
                    return true;
                }

                value = default;
                return false;
            }
        }

        public virtual void SetValue(object item, string path, object? value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                dictionary[path] = value;
            }
        }

        public virtual bool Contains(object item, string path)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        public virtual IReadOnlyList<KeyValuePair<string, object?>> GetValues(object item, Func<string, object?, bool>? predicate)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return Default.EmptyArray<KeyValuePair<string, object?>>();
            lock (dictionary)
            {
                if (predicate == null)
                    return new List<KeyValuePair<string, object?>>(dictionary);
                var list = new List<KeyValuePair<string, object?>>();
                foreach (var keyValue in dictionary)
                {
                    if (predicate(keyValue.Key, keyValue.Value))
                        list.Add(keyValue);
                }

                return list;
            }
        }

        public virtual bool Clear(object item)
        {
            Should.NotBeNull(item, nameof(item));
            return ClearInternal(item);
        }

        public virtual bool Clear(object item, string path)
        {
            var dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                if (dictionary.Remove(path))
                {
                    if (dictionary.Count == 0)
                        ClearInternal(item);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        protected static LightDictionaryBase<string, object?> GetOrAddAttachedValues(NotifyPropertyChangedBase model, bool addNew)
        {
            if (addNew && model.AttachedValues == null)
                Interlocked.CompareExchange(ref model.AttachedValues, new AttachedValueDictionary(), null);
            return model.AttachedValues;
        }

        protected static void ClearAttachedValues(NotifyPropertyChangedBase model)
        {
            model.AttachedValues?.Clear();
        }

        protected abstract bool ClearInternal(object item);

        protected abstract LightDictionaryBase<string, object?> GetOrAddAttachedDictionary(object item, bool addNew);

        #endregion

        #region Nested types

        public class AttachedValueDictionary : LightDictionaryBase<string, object?>
        {
            #region Constructors

            public AttachedValueDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,object>

            protected override bool Equals(string x, string y)
            {
                return x.Equals(y);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}