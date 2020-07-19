using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
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

        public Func<Type, MemberType, MemberFlags, object, IReadOnlyMetadataContext?, ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>>? TryGetMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> IMemberManagerComponent.TryGetMembers(IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _memberManager?.ShouldEqual(memberManager);
            return TryGetMembers?.Invoke(type, memberTypes, flags, request, metadata) ?? default;
        }

        #endregion
    }
}