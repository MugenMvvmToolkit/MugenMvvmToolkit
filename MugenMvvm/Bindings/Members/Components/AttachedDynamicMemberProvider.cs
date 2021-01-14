using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class AttachedDynamicMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        private readonly List<Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?>> _dynamicMembers;

        [Preserve(Conditional = true)]
        public AttachedDynamicMemberProvider()
        {
            _dynamicMembers = new List<Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?>>();
        }

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        public void Register(Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            _dynamicMembers.Add(getMember);
            OwnerOptional.TryInvalidateCache();
        }

        public void Unregister(Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?> getMember)
        {
            Should.NotBeNull(getMember, nameof(getMember));
            if (_dynamicMembers.Remove(getMember))
                OwnerOptional.TryInvalidateCache();
        }

        public void Clear()
        {
            _dynamicMembers.Clear();
            OwnerOptional.TryInvalidateCache();
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (_dynamicMembers.Count == 0)
                return default;
            var members = new ItemOrListEditor<IMemberInfo>();
            for (var i = 0; i < _dynamicMembers.Count; i++)
                members.AddIfNotNull(_dynamicMembers[i].Invoke(type, name, memberTypes, metadata)!);
            return members.ToItemOrList();
        }
    }
}