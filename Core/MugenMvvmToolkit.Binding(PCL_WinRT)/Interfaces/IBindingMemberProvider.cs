#region Copyright
// ****************************************************************************
// <copyright file="IBindingMemberProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the binding member provider.
    /// </summary>
    public interface IBindingMemberProvider
    {
        /// <summary>
        ///     Gets an instance of <see cref="IBindingMemberInfo" /> using the source type and binding path.
        /// </summary>
        /// <param name="sourceType">The specified source type.</param>
        /// <param name="path">The specified binding path.</param>
        /// <param name="ignoreAttachedMembers">If <c>true</c> provider ignores attached members.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the member cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>The instance of <see cref="IBindingMemberInfo" />.</returns>
        IBindingMemberInfo GetBindingMember([NotNull] Type sourceType, [NotNull] string path, bool ignoreAttachedMembers,
            bool throwOnError);

        /// <summary>
        ///     Registers the specified member.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <param name="member">The specified member.</param>
        /// <param name="rewrite"><c>true</c> rewrite exist member, <c>false</c> throw an exception.</param>
        void Register([NotNull] Type type, [NotNull] IBindingMemberInfo member, bool rewrite);

        /// <summary>
        ///     Unregisters the specified member using the type and member path.
        /// </summary>
        bool Unregister([NotNull] Type type, [NotNull] string path);

        /// <summary>
        ///     Unregisters all members using the type.
        /// </summary>
        bool Unregister([NotNull] Type type);
    }
}