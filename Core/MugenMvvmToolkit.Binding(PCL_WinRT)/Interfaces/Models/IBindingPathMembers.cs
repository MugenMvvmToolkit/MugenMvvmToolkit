#region Copyright

// ****************************************************************************
// <copyright file="IBindingPathMembers.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the binding member value, the implementation of this interface should be weak.
    /// </summary>
    public interface IBindingPathMembers
    {
        /// <summary>
        ///     Gets the <see cref="IBindingPath" />.
        /// </summary>
        [NotNull]
        IBindingPath Path { get; }

        /// <summary>
        ///     Gets the value that indicates that all members are available.
        /// </summary>
        bool AllMembersAvailable { get; }

        /// <summary>
        ///     Gets the available members.
        /// </summary>
        [NotNull]
        IList<IBindingMemberInfo> Members { get; }

        /// <summary>
        ///     Gets the last value, if all members is available; otherwise returns the empty value.
        /// </summary>
        [NotNull]
        IBindingMemberInfo LastMember { get; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        [CanBeNull]
        object Source { get; }

        /// <summary>
        ///     Gets the penultimate value.
        /// </summary>
        [CanBeNull]
        object PenultimateValue { get; }
    }
}