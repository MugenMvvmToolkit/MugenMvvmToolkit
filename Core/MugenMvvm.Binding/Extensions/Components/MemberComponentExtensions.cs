using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetMember(type, name, memberTypes, flags, metadata, out member))
                    return true;
            }

            member = null;
            return false;
        }

        public static bool TryGetMembers(this IMemberProviderComponent[] components, Type type, string name, MemberType memberTypes, MemberFlags flags,
            IReadOnlyMetadataContext? metadata, [NotNullWhen(true)] out IReadOnlyList<IMemberInfo>? members)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetMembers(type, name, memberTypes, flags, metadata, out members))
                    return true;
            }

            members = null;
            return false;
        }

        public static void TryAddMembers(this IRawMemberProviderComponent[] components, ICollection<IMemberInfo> result, Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(type, name, metadata);
                if (members == null || members.Count == 0)
                    continue;

                for (int j = 0; j < members.Count; j++)
                    result.Add(members[j]);
            }
        }

        public static IReadOnlyList<IMemberInfo>? TrySelectMembers(this ISelectorMemberProviderComponent[] components, IReadOnlyList<IMemberInfo> members,
            Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
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