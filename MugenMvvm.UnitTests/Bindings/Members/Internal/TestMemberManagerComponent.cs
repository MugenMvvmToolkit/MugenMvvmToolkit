using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public class TestMemberManagerComponent : IMemberManagerComponent, IHasPriority
    {
        public static readonly TestMemberManagerComponent Selector = new()
        {
            TryGetMembers = (type, memberType, arg3, arg4, arg6) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(arg4)
        };

        private readonly IMemberManager? _memberManager;

        public TestMemberManagerComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        public Func<Type, EnumFlags<MemberType>, EnumFlags<MemberFlags>, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IMemberInfo>>? TryGetMembers { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IMemberInfo> IMemberManagerComponent.TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes,
            EnumFlags<MemberFlags> flags, object request, IReadOnlyMetadataContext? metadata)
        {
            _memberManager?.ShouldEqual(memberManager);
            return TryGetMembers?.Invoke(type, memberTypes, flags, request, metadata) ?? default;
        }
    }
}