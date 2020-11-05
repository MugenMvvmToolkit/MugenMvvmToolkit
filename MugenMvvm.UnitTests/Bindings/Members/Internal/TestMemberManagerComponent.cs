using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public class TestMemberManagerComponent : IMemberManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberManager? _memberManager;

        public static readonly TestMemberManagerComponent Selector = new TestMemberManagerComponent
        {
            TryGetMembers = (type, memberType, arg3, arg4, arg6) => ItemOrList.FromRawValue<IMemberInfo, IReadOnlyList<IMemberInfo>>(arg4)
        };

        #endregion

        #region Constructors

        public TestMemberManagerComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<Type, EnumFlags<MemberType>, MemberFlags, object, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberManagerComponent.TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, MemberFlags flags, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _memberManager?.ShouldEqual(memberManager);
            return TryGetMembers?.Invoke(type, memberTypes, flags, request, metadata) ?? default;
        }

        #endregion
    }
}