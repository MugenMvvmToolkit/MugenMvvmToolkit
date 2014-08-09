#region Copyright
// ****************************************************************************
// <copyright file="IDisplayNameProvider.cs">
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

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents interface that provide display name of object.
    /// </summary>
    public interface IDisplayNameProvider
    {
        /// <summary>
        ///     Gets a display name for the specified type using the specified member.
        /// </summary>
        /// <param name="memberInfo">The specified member.</param>
        /// <returns>
        ///     An instance of string.
        /// </returns>
        [Pure, NotNull]
        Func<string> GetDisplayNameAccessor([NotNull] MemberInfo memberInfo);
    }
}