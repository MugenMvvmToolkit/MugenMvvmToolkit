using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class MemberComponentExtensions
    {
        #region Methods

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(this IMemberManagerComponent[] components, IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var members = components[i].TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);
                if (!members.IsEmpty)
                    return members;
            }

            return default;
        }

        public static void TryAddMembers(this IMemberProviderComponent[] components, IMemberManager memberManager, ICollection<IMemberInfo> result, Type type, string name, MemberType memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(result, nameof(result));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            for (var i = 0; i < components.Length; i++)
            {
                foreach (var member in components[i].TryGetMembers(memberManager, type, name, memberTypes, metadata))
                    result.Add(member);
            }
        }

        public static ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(this IMemberProviderComponent[] components, IMemberManager memberManager, Type type, string name, MemberType memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (components.Length == 1)
                return components[0].TryGetMembers(memberManager, type, name, memberTypes, metadata);
            var result = ItemOrListEditor.Get<IMemberInfo>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMembers(memberManager, type, name, memberTypes, metadata));
            return result.ToItemOrList<IReadOnlyList<IMemberInfo>>();
        }

        #endregion
    }
}