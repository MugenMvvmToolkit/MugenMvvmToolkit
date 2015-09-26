#region Copyright

// ****************************************************************************
// <copyright file="BindingActionValue.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public class BindingActionValue
    {
        #region Fields

        private readonly IBindingMemberInfo _member;
        private readonly WeakReference _memberSource;

        #endregion

        #region Constructors

        public BindingActionValue([NotNull] object memberSource, [NotNull] IBindingMemberInfo member)
        {
            Should.NotBeNull(memberSource, "memberSource");
            Should.NotBeNull(member, "member");
            _memberSource = ToolkitExtensions.GetWeakReference(memberSource);
            _member = member;
        }

        #endregion

        #region Properties

        [NotNull]
        public WeakReference MemberSource
        {
            get { return _memberSource; }
        }

        [NotNull]
        public IBindingMemberInfo Member
        {
            get { return _member; }
        }

        #endregion
    }
}
