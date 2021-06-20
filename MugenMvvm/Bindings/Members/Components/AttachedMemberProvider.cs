using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
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
    public sealed class AttachedMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        private readonly Dictionary<string, List<IMemberInfo>> _registeredMembers;

        [Preserve(Conditional = true)]
        public AttachedMemberProvider()
        {
            _registeredMembers = new Dictionary<string, List<IMemberInfo>>(59, StringComparer.Ordinal);
        }

        public int Priority { get; set; } = MemberComponentPriority.Attached;

        public ItemOrIReadOnlyList<IMemberInfo> GetAttachedMembers()
        {
            var members = new ItemOrListEditor<IMemberInfo>();
            foreach (var keyValuePair in _registeredMembers)
                members.AddRange(new ItemOrIEnumerable<IMemberInfo>(keyValuePair.Value));
            return members.ToItemOrList();
        }

        public void Register(IMemberInfo member, string? name = null)
        {
            Should.NotBeNull(member, nameof(member));
            name ??= member.Name;
            if (!_registeredMembers.TryGetValue(name, out var list))
            {
                list = new List<IMemberInfo>();
                _registeredMembers[name] = list;
            }

            list.Add(member);
            OwnerOptional?.TryInvalidateCache();
        }

        public void Unregister(IMemberInfo member)
        {
            Should.NotBeNull(member, nameof(member));
            var removed = false;
            foreach (var pair in _registeredMembers)
            {
                if (pair.Value.Remove(member))
                    removed = true;
            }

            if (removed)
                OwnerOptional?.TryInvalidateCache();
        }

        public void Clear()
        {
            _registeredMembers.Clear();
            OwnerOptional?.TryInvalidateCache();
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (!_registeredMembers.TryGetValue(name, out var members))
                return default;

            var result = new ItemOrListEditor<IMemberInfo>();
            for (var index = 0; index < members.Count; index++)
            {
                var memberInfo = members[index];
                if (memberTypes.HasFlag(memberInfo.MemberType) && memberInfo.DeclaringType.IsAssignableFromGeneric(type))
                    result.Add(memberInfo);
            }

            return result.ToItemOrList();
        }
    }
}