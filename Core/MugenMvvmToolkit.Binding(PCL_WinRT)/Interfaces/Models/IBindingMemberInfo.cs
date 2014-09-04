#region Copyright
// ****************************************************************************
// <copyright file="IBindingMemberInfo.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the binding member info.
    /// </summary>
    public interface IBindingMemberInfo
    {
        /// <summary>
        ///     Gets the path of member.
        /// </summary>
        [NotNull]
        string Path { get; }

        /// <summary>
        ///     Gets the type of member.
        /// </summary>
        [NotNull]
        Type Type { get; }

        /// <summary>
        ///     Gets the underlying member, if any.
        /// </summary>
        [CanBeNull]
        MemberInfo Member { get; }

        /// <summary>
        ///     Gets the member type.
        /// </summary>
        [NotNull]
        BindingMemberType MemberType { get; }

        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        ///     Returns the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be returned.</param>
        /// <param name="args">Optional values for members.</param>
        /// <returns>The member value of the specified object.</returns>
        object GetValue(object source, [CanBeNull] object[] args);

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="args">Optional values for members.</param>
        /// <returns>The member value of the specified object.</returns>
        object SetValue(object source, object[] args);

        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        [CanBeNull]
        IDisposable TryObserve(object source, [NotNull]IEventListener listener);
    }
}