using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class TestMemberManagerComponent : IMemberManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<Type, string, MemberType, MemberFlags, IReadOnlyMetadataContext?, IMemberInfo?>? TryGetMember { get; set; }

        public Func<Type, string, MemberType, MemberFlags, IReadOnlyMetadataContext?, IReadOnlyList<IMemberInfo>>? TryGetMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMemberManagerComponent.TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IMemberInfo? member)
        {
            member = TryGetMember?.Invoke(type, name, memberTypes, flags, metadata);
            return TryGetMember != null;
        }

        IReadOnlyList<IMemberInfo>? IMemberManagerComponent.TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMembers?.Invoke(type, name, memberTypes, flags, metadata);
        }

        #endregion
    }
}