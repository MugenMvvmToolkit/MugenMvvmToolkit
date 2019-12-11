using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MemberProviderComponent : AttachableComponentBase<IMemberProvider>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly List<IMemberInfo> _members;

        #endregion

        #region Constructors

        public MemberProviderComponent()
        {
            _members = new List<IMemberInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Provider;

        #endregion

        #region Implementation of interfaces

        public bool TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IMemberInfo? member)
        {
            _members.Clear();
            Owner.GetComponents<IRawMemberProviderComponent>(metadata).TryAddMembers(_members, type, name, metadata);

            var members = Owner.GetComponents<ISelectorMemberProviderComponent>(metadata).TrySelectMembers(_members, type, name, memberTypes, flags, metadata);
            if (members != null)
            {
                member = members.Count == 0 ? null : members[0];
                return true;
            }

            member = null;
            return false;
        }

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            _members.Clear();
            Owner.GetComponents<IRawMemberProviderComponent>(metadata).TryAddMembers(_members, type, name, metadata);

            return Owner.GetComponents<ISelectorMemberProviderComponent>(metadata).TrySelectMembers(_members, type, name, memberTypes, flags, metadata);
        }

        #endregion
    }
}