#region Copyright
// ****************************************************************************
// <copyright file="IAttachedValueProvider.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the attached value provider interface, that allows to attach a value to an object using path.
    /// </summary>
    public interface IAttachedValueProvider
    {
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
        TValue AddOrUpdate<TItem, TValue>([NotNull] TItem item, [NotNull] string path, [CanBeNull] TValue addValue,
            [NotNull] UpdateValueDelegate<TItem, TValue, TValue, object> updateValueFactory, object state = null);

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
        TValue AddOrUpdate<TItem, TValue>([NotNull] TItem item, [NotNull] string path,
            [NotNull] Func<TItem, object, TValue> addValueFactory,
            [NotNull] UpdateValueDelegate<TItem, Func<TItem, object, TValue>, TValue, object> updateValueFactory,
            object state = null);

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
        TValue GetOrAdd<TItem, TValue>([NotNull] TItem item, [NotNull] string path,
            [NotNull] Func<TItem, object, TValue> valueFactory, object state);

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
        TValue GetOrAdd<TValue>([NotNull] object item, [NotNull] string path, [CanBeNull] TValue value);

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
        bool TryGetValue<TValue>([NotNull] object item, [NotNull] string path, out TValue value);

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
        TValue GetValue<TValue>([NotNull] object item, [NotNull] string path, bool throwOnError);

        /// <summary>
        ///     Sets the value associated with the specified property.
        /// </summary>
        /// <returns>
        ///     The value of the property.
        /// </returns>
        /// <param name="item">The item of the value to set.</param>
        /// <param name="path">The path of the value to set.</param>
        /// <param name="value">The value to set.</param>
        void SetValue([NotNull] object item, [NotNull] string path, [CanBeNull] object value);

        /// <summary>
        ///     Determines whether the <see cref="IAttachedValueProvider" /> contains the specified key.
        /// </summary>
        /// <param name="item">The item of the value to set.</param>
        /// <param name="path">The path of the value to set.</param>
        bool Contains([NotNull] object item, [NotNull] string path);

        /// <summary>
        ///     Gets the property values for the specified item
        /// </summary>
        [NotNull]
        IList<KeyValuePair<string, object>> GetValues([NotNull] object item, [CanBeNull] Func<string, object, bool> predicate);

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        bool Clear([NotNull] object item);

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        bool Clear([NotNull] object item, [NotNull] string path);
    }
}