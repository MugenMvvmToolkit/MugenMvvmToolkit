#region Copyright

// ****************************************************************************
// <copyright file="AttachedValueProviderBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
    /// <summary>
    ///     Represents the attached value provider class, that allows to attach a value to an object using path.
    /// </summary>
    public abstract class AttachedValueProviderBase : IAttachedValueProvider
    {
        #region Implementation of IAttachedValueProvider

        /// <summary>
        ///     Adds an attached property value to the <see cref="IAttachedValueProvider" /> if the property does not already
        ///     exist, or to
        ///     update an attached property in the <see cref="IAttachedValueProvider" /> if the property
        ///     already exists.
        /// </summary>
        /// <param name="item">The item to be added or whose value should be updated</param>
        /// <param name="path">The attached property path.</param>
        /// <param name="addValue">The value to be added for an absent property</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing property based on the item's existing value
        /// </param>
        /// <param name="state">The specified state object.</param>
        /// <returns>
        ///     The new value for the property. This will be either be addValue (if the property was absent) or the result of
        ///     updateValueFactory (if the property was present).
        /// </returns>
        public virtual TValue AddOrUpdate<TItem, TValue>(TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue, object> updateValueFactory,
            object state = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
            Should.NotBeNull(updateValueFactory, "updateValueFactory");
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

        /// <summary>
        ///     Uses the specified functions to add an attached property value to the
        ///     <see cref="IAttachedValueProvider" /> if the item does not already exist, or to
        ///     update an attached property in the <see cref="IAttachedValueProvider" /> if the property
        ///     already exists.
        /// </summary>
        /// <param name="item">The item to be added or whose value should be updated</param>
        /// <param name="path">The attached property path.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent property</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing property based on the item's existing value
        /// </param>
        /// <param name="state">The specified state object.</param>
        /// <returns>
        ///     The new value for the property. This will be either be the result of addValueFactory (if the property was absent)
        ///     or the
        ///     result of updateValueFactory (if the property was present).
        /// </returns>
        public virtual TValue AddOrUpdate<TItem, TValue>(TItem item, string path,
            Func<TItem, object, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, object, TValue>, TValue, object> updateValueFactory,
            object state = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
            Should.NotBeNull(addValueFactory, "addValueFactory");
            Should.NotBeNull(updateValueFactory, "updateValueFactory");
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

        /// <summary>
        ///     Adds an attached property value to the <see cref="IAttachedValueProvider" /> if the property does not already
        ///     exist.
        /// </summary>
        /// <param name="item">The item of the element to add.</param>
        /// <param name="path">The attached property path.</param>
        /// <param name="valueFactory">The function used to generate a value for the item</param>
        /// <param name="state">The specified state object.</param>
        /// <returns>
        ///     The value for the property. This will be either the existing value for the property if the property is already in
        ///     the provider,
        ///     or the new value if the property was not in the provider
        /// </returns>
        public virtual TValue GetOrAdd<TItem, TValue>(TItem item, string path, Func<TItem, object, TValue> valueFactory,
            object state = null)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
            Should.NotBeNull(valueFactory, "valueFactory");
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

        /// <summary>
        ///     Adds an attached property value to the <see cref="IAttachedValueProvider" /> if the property does not already
        ///     exist.
        /// </summary>
        /// <param name="item">The item of the element to add.</param>
        /// <param name="path">The attached property path.</param>
        /// <param name="value">the value to be added, if the item does not already exist</param>
        /// <returns>
        ///     The value for the property. This will be either the existing value for the property if the property is already in
        ///     the provider,
        ///     or the new value if the property was not in the provider
        /// </returns>
        public virtual TValue GetOrAdd<TValue>(object item, string path, TValue value)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
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

        /// <summary>
        ///     Attempts to get the value associated with the specified property from the <see cref="IAttachedValueProvider" />.
        /// </summary>
        /// <returns>
        ///     true if the property was found in the <see cref="IAttachedValueProvider" />; otherwise, false.
        /// </returns>
        /// <param name="item">The item of the value to get.</param>
        /// <param name="path">The attached property path.</param>
        /// <param name="value">
        ///     When this method returns, contains the object from the
        ///     <see cref="IAttachedValueProvider" /> that has the specified property, or null value, if the operation failed.
        /// </param>
        public virtual bool TryGetValue<TValue>(object item, string path, out TValue value)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
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

        /// <summary>
        ///     Gets the value associated with the specified property.
        /// </summary>
        /// <returns>
        ///     The value of the property.
        /// </returns>
        /// <param name="item">The item of the value to get.</param>
        /// <param name="path">The path of the value to get.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the member cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        public virtual TValue GetValue<TValue>(object item, string path, bool throwOnError)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
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

        /// <summary>
        ///     Sets the value associated with the specified property.
        /// </summary>
        /// <returns>
        ///     The value of the property.
        /// </returns>
        /// <param name="item">The item of the value to set.</param>
        /// <param name="path">The path of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public virtual void SetValue(object item, string path, object value)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(path, "path");
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, true);
            lock (dictionary)
                dictionary[path] = value;
        }

        /// <summary>
        ///     Determines whether the <see cref="IAttachedValueProvider" /> contains the specified key.
        /// </summary>
        /// <param name="item">The item of the value to set.</param>
        /// <param name="path">The path of the value to set.</param>
        public virtual bool Contains(object item, string path)
        {
            Should.NotBeNull(item, "item");
            LightDictionaryBase<string, object> dictionary = GetOrAddAttachedDictionary(item, false);
            if (dictionary == null)
                return false;
            lock (dictionary)
                return dictionary.ContainsKey(path);
        }

        /// <summary>
        ///     Gets the property values for the specified item
        /// </summary>
        public virtual IList<KeyValuePair<string, object>> GetValues(object item, Func<string, object, bool> predicate)
        {
            Should.NotBeNull(item, "item");
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

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        public virtual bool Clear(object item)
        {
            Should.NotBeNull(item, "item");
            return ClearInternal(item);
        }

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
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

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        protected abstract bool ClearInternal(object item);

        /// <summary>
        ///     Gets or adds the attached values container.
        /// </summary>
        protected abstract LightDictionaryBase<string, object> GetOrAddAttachedDictionary(object item, bool addNew);

        #endregion
    }
}