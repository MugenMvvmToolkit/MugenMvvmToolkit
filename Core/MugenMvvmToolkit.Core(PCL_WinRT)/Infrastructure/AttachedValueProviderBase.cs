#region Copyright

// ****************************************************************************
// <copyright file="AttachedValueProviderBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class AttachedValueProviderBase : IAttachedValueProvider
    {
        #region Implementation of IAttachedValueProvider

        public virtual TValue AddOrUpdate<TItem, TValue>(TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue, object> updateValueFactory,
            object state = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                object value;
                if (dictionary.TryGetValue(path, out value))
                {
                    value = updateValueFactory(item, addValue, (TValue)value, state);
                    dictionary[path] = value;
                    return (TValue)value;
                }
                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public virtual TValue AddOrUpdate<TItem, TValue>(TItem item, string path,
            Func<TItem, object, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, object, TValue>, TValue, object> updateValueFactory,
            object state = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                object value;
                if (dictionary.TryGetValue(path, out value))
                {
                    value = updateValueFactory(item, addValueFactory, (TValue)value, state);
                    dictionary[path] = value;
                    return (TValue)value;
                }
                value = addValueFactory(item, state);
                dictionary.Add(path, value);
                return (TValue)value;
            }
        }

        public virtual TValue GetOrAdd<TItem, TValue>(TItem item, string path, Func<TItem, object, TValue> valueFactory,
            object state = null)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                object oldValue;
                if (dictionary.TryGetValue(path, out oldValue))
                    return (TValue)oldValue;
                oldValue = valueFactory(item, state);
                dictionary.Add(path, oldValue);
                return (TValue)oldValue;
            }
        }

        public virtual TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
            {
                object oldValue;
                if (dictionary.TryGetValue(path, out oldValue))
                    return (TValue)oldValue;
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual bool TryGetValue<TValue>(object item, string path, out TValue value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
            {
                value = default(TValue);
                return false;
            }
            lock (dictionary)
            {
                object result;
                if (dictionary.TryGetValue(path, out result))
                {
                    value = (TValue)result;
                    return true;
                }
                value = default(TValue);
                return false;
            }
        }

        public virtual TValue GetValue<TValue>(object item, string path, bool throwOnError)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
            {
                if (throwOnError)
                    throw new KeyNotFoundException();
                return default(TValue);
            }
            lock (dictionary)
            {
                object value;
                if (!dictionary.TryGetValue(path, out value))
                {
                    if (throwOnError)
                        throw new KeyNotFoundException();
                    return default(TValue);
                }
                return (TValue)value;
            }
        }

        public virtual void SetValue(object item, string path, object value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
                dictionary[path] = value;
        }

        public virtual bool Contains(object item, string path)
        {
            Should.NotBeNull(item, nameof(item));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            lock (dictionary)
                return dictionary.ContainsKey(path);
        }

        public virtual IList<KeyValuePair<string, object>> GetValues(object item, Func<string, object, bool> predicate)
        {
            Should.NotBeNull(item, nameof(item));
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return Empty.Array<KeyValuePair<string, object>>();
            lock (dictionary)
            {
                if (predicate == null)
                    return new List<KeyValuePair<string, object>>(dictionary);
                var list = new List<KeyValuePair<string, object>>();
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
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
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

        protected abstract bool ClearInternal(object item);

        protected abstract LightDictionaryBase<string, object> GetOrAddAttachedDictionary(object item, bool addNew);

        #endregion
    }
}
