using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class FakeMemberProvider : IMemberProviderComponent, IHasPriority
    {
        public const char FakeMemberPrefixSymbol = '#';
        public const string FakeMemberPrefix = "Fake";

        private readonly Dictionary<string, ConstantMemberInfo> _cache;

        [Preserve(Conditional = true)]
        public FakeMemberProvider()
        {
            _cache = new Dictionary<string, ConstantMemberInfo>(7, StringComparer.Ordinal);
        }

        public int Priority { get; init; } = MemberComponentPriority.Dynamic;

        public static bool IsFakeMember(string name) => name.Length != 0 && (name[0] == FakeMemberPrefixSymbol || name.StartsWith(FakeMemberPrefix, StringComparison.Ordinal));

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Accessor) || !IsFakeMember(name))
                return default;

            if (!_cache.TryGetValue(name, out var value))
            {
                value = new ConstantMemberInfo(name, null, true);
                _cache[name] = value;
            }

            return value;
        }
    }
}