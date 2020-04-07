using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class MemberComponentExtensions
    {
        #region Methods

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(this IMemberManagerComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(request, metadata);
                if (!members.IsNullOrEmpty())
                    return members;
            }

            return default;
        }

        public static void TryAddMembers(this IMemberProviderComponent[] components, ICollection<IMemberInfo> result, Type type, string name, IReadOnlyMetadataContext? metadata)
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

        public static IReadOnlyList<IMemberInfo>? TryGetMembers(this IMemberProviderComponent[] components, Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            LazyList<IMemberInfo> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMembers(type, name, metadata));
            return result.List;
        }

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TrySelectMembers(this IMemberSelectorComponent[] components, IReadOnlyList<IMemberInfo> members,
            Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(members, nameof(members));
            Should.NotBeNull(type, nameof(type));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TrySelectMembers(members, type, memberTypes, flags, metadata);
                if (!result.IsNullOrEmpty())
                    return result;
            }

            return default;
        }

        #endregion
    }
}