#region Copyright
// ****************************************************************************
// <copyright file="BindingMemberValue.cs">
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

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the dynamic binding member value.
    /// </summary>
    public class BindingMemberValue
    {
        #region Fields

        private readonly IBindingMemberInfo _member;
        private readonly WeakReference _memberSource;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberValue" /> class.
        /// </summary>
        public BindingMemberValue([NotNull] object memberSource, [NotNull] IBindingMemberInfo member)
        {
            Should.NotBeNull(memberSource, "memberSource");
            Should.NotBeNull(member, "member");
            _memberSource = MvvmExtensions.GetWeakReference(memberSource);
            _member = member;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the member source.
        /// </summary>
        [NotNull]
        public WeakReference MemberSource
        {
            get { return _memberSource; }
        }

        /// <summary>
        ///     Gets the current member.
        /// </summary>
        [NotNull]
        public IBindingMemberInfo Member
        {
            get { return _member; }
        }

        #endregion
    }
}