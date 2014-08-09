#region Copyright
// ****************************************************************************
// <copyright file="IBindingPath.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents a data structure for describing a member as a path below another member, or below an owning type.
    /// </summary>
    public interface IBindingPath
    {
        /// <summary>
        ///     Gets the string that describes the path.
        /// </summary>
        [NotNull]
        string Path { get; }

        /// <summary>
        ///     Gets the path members.
        /// </summary>
        [NotNull]
        IList<string> Parts { get; }

        /// <summary>
        ///     Gets the value that indicates that path is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        ///     Gets the value that indicates that path has a single item.
        /// </summary>
        bool IsSingle { get; }
    }
}