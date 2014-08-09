#region Copyright
// ****************************************************************************
// <copyright file="UnsetBindingPathMembers.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the unset <see cref="IBindingPathMembers" />.
    /// </summary>
    public sealed class UnsetBindingPathMembers : IBindingPathMembers
    {
        #region Fields

        /// <summary>
        ///     Gets the empty path members instance.
        /// </summary>
        public static readonly IBindingPathMembers Instance = new UnsetBindingPathMembers();

        #endregion

        #region Constructors

        private UnsetBindingPathMembers()
        {
        }

        #endregion

        #region Implementation of IBindingPathMembers

        /// <summary>
        ///     Gets the <see cref="IBindingPath" />.
        /// </summary>
        public IBindingPath Path
        {
            get { return BindingPath.None; }
        }

        /// <summary>
        ///     Gets the value that indicates that all members are available, if <c>true</c>.
        /// </summary>
        public bool AllMembersAvailable
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the available members.
        /// </summary>
        public IList<IBindingMemberInfo> Members
        {
            get { return EmptyValue<IBindingMemberInfo>.ListInstance; }
        }

        /// <summary>
        ///     Gets the last value, if all members is available; otherwise returns the empty value.
        /// </summary>
        public IBindingMemberInfo LastMember
        {
            get { return BindingMemberInfo.Unset; }
        }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        public object Source
        {
            get { return BindingConstants.UnsetValue; }
        }

        /// <summary>
        ///     Gets the penultimate value.
        /// </summary>
        public object PenultimateValue
        {
            get { return Source; }
        }

        #endregion
    }
}