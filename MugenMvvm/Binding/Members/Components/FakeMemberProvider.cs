using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class FakeMemberProvider : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, ConstantMemberInfo> _cache;

        public const char FakeMemberPrefixSymbol = '#';
        public const string FakeMemberPrefix = "Fake";

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public FakeMemberProvider()
        {
            _cache = new Dictionary<string, ConstantMemberInfo>(7, StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Dynamic;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlagEx(MemberType.Accessor) || !IsFakeMember(name))
                return default;

            if (!_cache.TryGetValue(name, out var value))
            {
                value = new ConstantMemberInfo(name, null, true);
                _cache[name] = value;
            }

            return value;
        }

        #endregion

        #region Methods

        public static bool IsFakeMember(string name) => name.Length != 0 && (name[0] == FakeMemberPrefixSymbol || name.StartsWith(FakeMemberPrefix, StringComparison.Ordinal));

        #endregion
    }
}