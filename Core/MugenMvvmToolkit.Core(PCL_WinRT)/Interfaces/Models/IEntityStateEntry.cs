#region Copyright
// ****************************************************************************
// <copyright file="IEntityStateEntry.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the entity state entry.
    /// </summary>
    public interface IEntityStateEntry
    {
        /// <summary>
        ///     Gets or sets the state of the <see cref="EntityState" />.
        /// </summary>
        EntityState State { get; }

        /// <summary>
        ///     Gets the entity object.
        /// </summary>
        [NotNull]
        object Entity { get; }
    }
}