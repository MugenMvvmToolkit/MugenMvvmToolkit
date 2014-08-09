#region Copyright
// ****************************************************************************
// <copyright file="INotifiableCollection.cs">
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
using System.Collections;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    /// <summary>
    ///     Represents the notifiable collection interface.
    /// </summary>
    public interface INotifiableCollection : IList, INotifyCollectionChanging, ISuspendNotifications
    {
        /// <summary>
        ///     Raises a <c>CollectionChanged</c> event of type reset.
        /// </summary>
        void RaiseReset();

        /// <summary>
        ///     Adds the specified items to the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        void AddRange(IEnumerable collection);
    }

    /// <summary>
    ///     Represents the notifiable collection interface.
    /// </summary>
    public interface INotifiableCollection<T> : IList<T>, INotifyCollectionChanging, ISuspendNotifications
    {
        /// <summary>
        ///     Raises a <c>CollectionChanged</c> event of type reset.
        /// </summary>
        void RaiseReset();

        /// <summary>
        ///     Adds the specified items to the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        void AddRange(IEnumerable<T> collection);

        /// <summary>
        ///     Clears collection and then adds a range of IEnumerable collection.
        /// </summary>
        /// <param name="items">Items to add</param>
        void Update(IEnumerable<T> items);
    }
}