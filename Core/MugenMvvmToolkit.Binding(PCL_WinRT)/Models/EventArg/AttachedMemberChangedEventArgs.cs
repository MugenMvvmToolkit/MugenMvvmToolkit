#region Copyright

// ****************************************************************************
// <copyright file="AttachedMemberChangedEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class AttachedMemberChangedEventArgs<T> : ValueChangedEventArgs<T>
    {
        #region Fields

        private readonly IBindingMemberInfo _member;
        private readonly object[] _args;

        #endregion

        #region Constructors

        public AttachedMemberChangedEventArgs(T oldValue, T newValue, object[] args, [NotNull] IBindingMemberInfo member)
            : base(oldValue, newValue)
        {
            Should.NotBeNull(member, "member");
            _member = member;
            _args = args;
        }

        #endregion

        #region Properties

        [NotNull]
        public IBindingMemberInfo Member
        {
            get { return _member; }
        }

        [CanBeNull]
        public object[] Args
        {
            get { return _args; }
        }

        #endregion
    }
}
