using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestMemberSelectorComponent : IMemberSelectorComponent, IHasPriority
    {
        #region Properties

        public Func<IReadOnlyList<IMemberInfo>, Type, MemberType, MemberFlags, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TrySelectMembers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberSelectorComponent.TrySelectMembers(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags,
            IReadOnlyMetadataContext? metadata)
        {
            return TrySelectMembers?.Invoke(members, type, memberTypes, flags, metadata) ?? default;
        }

        #endregion
    }
}