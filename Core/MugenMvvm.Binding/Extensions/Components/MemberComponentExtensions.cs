using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class MemberComponentExtensions
    {
        #region Methods

        public static bool TryGetMember(this IMemberProviderComponent[] components, Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IMemberInfo? member)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetMember(type, name, memberTypes, flags, metadata, out member))
                    return true;
            }

            member = null;
            return false;
        }

        public static IReadOnlyList<IMemberInfo>? TryGetMembers(this IMemberProviderComponent[] components, Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(type, name, memberTypes, flags, metadata);
                if (members != null)
                    return members;
            }

            return null;
        }

        public static void TryAddMembers(this IRawMemberProviderComponent[] components, ICollection<IMemberInfo> result, Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(result, nameof(result));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(type, name, metadata);
                if (members == null || members.Count == 0)
                    continue;

                for (var j = 0; j < members.Count; j++)
                    result.Add(members[j]);
            }
        }

        public static IReadOnlyList<IMemberInfo>? TrySelectMembers(this ISelectorMemberProviderComponent[] components, IReadOnlyList<IMemberInfo> members,
            Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(members, nameof(members));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TrySelectMembers(members, type, name, memberTypes, flags, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion
    }
}