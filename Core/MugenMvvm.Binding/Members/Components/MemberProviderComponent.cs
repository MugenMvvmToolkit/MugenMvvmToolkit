using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
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
            FillMembers(type, name, metadata);
            var selectors = Owner.Components.Get<ISelectorMemberProviderComponent>(metadata);
            for (var i = 0; i < selectors.Length; i++)
            {
                var members = selectors[i].TrySelectMembers(_members, type, name, memberTypes, flags, metadata);
                if (members != null)
                {
                    member = members.Count == 0 ? null : members[0];
                    return true;
                }
            }

            member = null;
            return false;
        }

        public bool TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IReadOnlyList<IMemberInfo>? members)
        {
            var selectors = Owner.Components.Get<ISelectorMemberProviderComponent>(metadata);
            for (var i = 0; i < selectors.Length; i++)
            {
                members = selectors[i].TrySelectMembers(_members, type, name, memberTypes, flags, metadata);
                if (members != null)
                    return true;
            }

            members = null;
            return false;
        }

        #endregion

        #region Methods

        private void FillMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            _members.Clear();
            var components = Owner.Components.Get<IRawMemberProviderComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(type, name, metadata);
                if (members != null && members.Count != 0)
                    _members.AddRange(members);
            }
        }

        #endregion
    }
}