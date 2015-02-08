#region Copyright

// ****************************************************************************
// <copyright file="ITrackingCollection.cs">
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
using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    /// <summary>
    ///     Represents the collection that allows to track the changes of entities.
    /// </summary>
    public interface ITrackingCollection : INotifyPropertyChanged, IEnumerable<TrackingEntity<object>>
    {
        /// <summary>
        ///     Gets or sets the <see cref="IStateTransitionManager" />.
        /// </summary>
        [NotNull]
        IStateTransitionManager StateTransitionManager { get; set; }

        /// <summary>
        ///     Gets or sets the property that indicates that the state will be validated before assigned.
        /// </summary>
        bool ValidateState { get; set; }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="ITrackingCollection" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="ITrackingCollection" />.
        /// </returns>
        int Count { get; }

        /// <summary>
        ///     Gets a value indicating whether the collection has changes, including new, deleted, or modified values.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        ///     Determines whether the <see cref="ITrackingCollection" /> contains a specific value.
        /// </summary>
        [Pure]
        bool Contains([NotNull] object item);

        /// <summary>
        ///     Determines whether the <see cref="ITrackingCollection" /> contains a specific value.
        /// </summary>
        [Pure]
        bool Contains<TEntity>([NotNull] Func<TrackingEntity<TEntity>, bool> predicate);

        /// <summary>
        ///     Gets an array of all objects with specified entity state.
        /// </summary>
        /// <returns>
        ///     An array of objects.
        /// </returns>
        [NotNull]
        IList<TEntity> Find<TEntity>([CanBeNull] Func<TrackingEntity<TEntity>, bool> predicate);

        /// <summary>
        ///     Gets the changes of objects.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IList{T}" />.
        /// </returns>
        [NotNull]
        IList<IEntityStateEntry> GetChanges(
            EntityState entityState = EntityState.Added | EntityState.Modified | EntityState.Deleted);

        /// <summary>
        ///     Gets the object state.
        /// </summary>
        /// <param name="value">The specified value.</param>
        /// <returns>
        ///     An instance of <see cref="EntityState" />.
        /// </returns>
        [Pure]
        EntityState GetState([NotNull] object value);

        /// <summary>
        ///     Updates a state in the specified value.
        /// </summary>
        /// <param name="value">The specified value to update state.</param>
        /// <param name="state">The state value.</param>
        /// <param name="validateState">The flag indicating that state will be validated before assigned.</param>
        /// <returns>
        ///     If <c>true</c> the state was changed; otherwise <c>false</c>.
        /// </returns>
        bool UpdateState([NotNull] object value, EntityState state, bool? validateState = null);

        /// <summary>
        ///     Removes all items from the <see cref="ITrackingCollection" />.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        [NotNull]
        ITrackingCollection Clone();
    }
}