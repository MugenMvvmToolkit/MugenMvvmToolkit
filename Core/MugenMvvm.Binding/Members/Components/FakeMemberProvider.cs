using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class FakeMemberProvider : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<ConstantMemberInfo> _cache;

        public const char FakeMemberPrefixSymbol = '#';
        public const string FakeMemberPrefix = "Fake";

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public FakeMemberProvider()
        {
            _cache = new StringOrdinalLightDictionary<ConstantMemberInfo>(7);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Dynamic;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            if (name.Length == 0 || name[0] != FakeMemberPrefixSymbol && !name.StartsWith(FakeMemberPrefix, StringComparison.Ordinal))
                return default;

            if (!_cache.TryGetValue(name, out var value))
            {
                value = new ConstantMemberInfo(name, null, true);
                _cache[name] = value;
            }

            return value;
        }

        #endregion
    }
}