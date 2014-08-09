#region Copyright
// ****************************************************************************
// <copyright file="IEntitySnapshot.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the entity state snapshot.
    /// </summary>
    public interface IEntitySnapshot
    {
        /// <summary>
        ///     Gets a value indicating whether the snapshot supports change detection.
        /// </summary>
        bool SupportChangeDetection { get; }

        /// <summary>
        ///     Restores the state of entity.
        /// </summary>
        /// <param name="entity">The specified entity to restore state.</param>
        void Restore([NotNull] object entity);

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        [Pure]
        bool HasChanges([NotNull] object entity);

        /// <summary>
        ///     Gets a value indicating whether the entity has changes.
        /// </summary>
        [Pure]
        bool HasChanges([NotNull] object entity, [NotNull] string propertyName);

        /// <summary>
        ///     Dumps the state of object.
        /// </summary>
        [NotNull]
        IDictionary<string, Tuple<object, object>> Dump([NotNull] object entity);
    }
}