#region Copyright
// ****************************************************************************
// <copyright file="MemberAttachedEventArgs.cs">
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

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class MemberAttachedEventArgs : EventArgs
    {
        #region Fields

        private readonly IBindingMemberInfo _member;

        #endregion

        #region Constructors

        public MemberAttachedEventArgs([NotNull] IBindingMemberInfo member)
        {
            Should.NotBeNull(member, "member");
            _member = member;
        }

        #endregion

        #region Properties

        [NotNull]
        public IBindingMemberInfo Member
        {
            get { return _member; }
        }

        #endregion
    }
}