using System.Collections.Generic;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public sealed class BindingPathMembersMock : IBindingPathMembers
    {
        #region Fields

        public static readonly IBindingPathMembers Unset = new BindingPathMembersMock(BindingConstants.UnsetValue, null,
            BindingConstants.UnsetValue, Empty.Array<IBindingMemberInfo>(), false);

        private IList<IBindingMemberInfo> _members;

        #endregion

        #region Constructors

        public BindingPathMembersMock(object source, IBindingPath path)
        {
            Path = path;
            AllMembersAvailable = source != null;
            Source = source;
            PenultimateValue = source;
            _members = Empty.Array<IBindingMemberInfo>();
            LastMember = BindingMemberInfo.Empty;
        }

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

        public IBindingPath Path { get; private set; }

        public bool AllMembersAvailable { get; private set; }

        public IList<IBindingMemberInfo> Members
        {
            get
            {
                if (_members == null)
                    _members = new[] { LastMember };
                return _members;
            }
        }

        public IBindingMemberInfo LastMember { get; private set; }

        public object Source { get; private set; }

        public object PenultimateValue { get; private set; }

        #endregion
    }
}
