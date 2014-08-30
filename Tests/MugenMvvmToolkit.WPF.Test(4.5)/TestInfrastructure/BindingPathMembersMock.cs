using System.Collections.Generic;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    /// <summary>
    ///     Represents the binding path members.
    /// </summary>
    public sealed class BindingPathMembersMock : IBindingPathMembers
    {
        #region Fields

        /// <summary>
        ///     Gets the empty value.
        /// </summary>
        public static readonly IBindingPathMembers Unset = new BindingPathMembersMock(BindingConstants.UnsetValue, null,
            BindingConstants.UnsetValue, Empty.Array<IBindingMemberInfo>(), false);

        private IList<IBindingMemberInfo> _members;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingPathMembersMock" /> class.
        /// </summary>
        public BindingPathMembersMock(object source, IBindingPath path)
        {
            Path = path;
            AllMembersAvailable = source != null;
            Source = source;
            PenultimateValue = source;
            _members = Empty.Array<IBindingMemberInfo>();
            LastMember = BindingMemberInfo.Empty;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingPathMembersMock" /> class.
        /// </summary>
        public BindingPathMembersMock(object source, IBindingPath path, IBindingMemberInfo member)
        {
            Source = source;
            PenultimateValue = source;
            Path = path;
            if (member == null || source == null)
            {
                AllMembersAvailable = false;
                LastMember = BindingMemberInfo.Unset;
                _members = Empty.Array<IBindingMemberInfo>();
            }
            else
            {
                AllMembersAvailable = true;
                LastMember = member;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingPathMembersMock" /> class.
        /// </summary>
        public BindingPathMembersMock(object source, IBindingPath path, object penultimateValue,
            IList<IBindingMemberInfo> members, bool allMembersAvailable)
        {
            Path = path;
            Source = source;
            PenultimateValue = penultimateValue;
            AllMembersAvailable = allMembersAvailable && penultimateValue != null;
            _members = members ?? Empty.Array<IBindingMemberInfo>();
            if (allMembersAvailable && _members.Count != 0)
                LastMember = _members[_members.Count - 1];
            else
                LastMember = BindingMemberInfo.Unset;
        }

        #endregion

        #region Implementation of IBindingPathMembers

        /// <summary>
        ///     Gets the <see cref="IBindingPath" />.
        /// </summary>
        public IBindingPath Path { get; private set; }

        /// <summary>
        ///     Gets the value that indicates that all members are available, if <c>true</c>.
        /// </summary>
        public bool AllMembersAvailable { get; private set; }

        /// <summary>
        ///     Gets the available members.
        /// </summary>
        public IList<IBindingMemberInfo> Members
        {
            get
            {
                if (_members == null)
                    _members = new[] { LastMember };
                return _members;
            }
        }

        /// <summary>
        ///     Gets the last value, if all members is available; otherwise returns the empty value.
        /// </summary>
        public IBindingMemberInfo LastMember { get; private set; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        public object Source { get; private set; }

        /// <summary>
        ///     Gets the penultimate value.
        /// </summary>
        public object PenultimateValue { get; private set; }

        #endregion
    }
}