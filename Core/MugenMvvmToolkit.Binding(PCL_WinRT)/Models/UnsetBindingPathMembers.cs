#region Copyright

// ****************************************************************************
// <copyright file="UnsetBindingPathMembers.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class UnsetBindingPathMembers : IBindingPathMembers
    {
        #region Fields

        public static readonly IBindingPathMembers Instance;

        #endregion

        #region Constructors

        static UnsetBindingPathMembers()
        {
            Instance = new UnsetBindingPathMembers();
        }

        private UnsetBindingPathMembers()
        {
        }

        #endregion

        #region Implementation of IBindingPathMembers

        public IBindingPath Path
        {
            get { return BindingPath.None; }
        }

        public bool AllMembersAvailable
        {
            get { return false; }
        }

        public IList<IBindingMemberInfo> Members
        {
            get { return Empty.Array<IBindingMemberInfo>(); }
        }

        public IBindingMemberInfo LastMember
        {
            get { return BindingMemberInfo.Unset; }
        }

        public object Source
        {
            get { return BindingConstants.UnsetValue; }
        }

        public object PenultimateValue
        {
            get { return BindingConstants.UnsetValue; }
        }

        #endregion
    }
}
