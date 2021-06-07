using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Members
{
    public class TestMemberManagerComponent : IMemberManagerComponent, IHasPriority
    {
        public static readonly TestMemberManagerComponent Selector = new()
        {
            TryGetMembers = (_, _, _, _, value, _) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(value)
        };

        public Func<IMemberManager, Type, EnumFlags<MemberType>, EnumFlags<MemberFlags>, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IMemberInfo>>? TryGetMembers
        {
            get;
            set;
        }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IMemberInfo> IMemberManagerComponent.TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes,
            EnumFlags<MemberFlags> flags, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetMembers?.Invoke(memberManager, type, memberTypes, flags, request, metadata) ?? default;
    }
}