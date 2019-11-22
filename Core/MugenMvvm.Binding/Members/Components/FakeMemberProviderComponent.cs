﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class FakeMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IReadOnlyList<IMemberInfo>> _cache;

        public const char FakeMemberPrefixSymbol = '#';
        public const string FakeMemberPrefix = "Fake";

        #endregion

        #region Constructors

        public FakeMemberProviderComponent()
        {
            _cache = new StringOrdinalLightDictionary<IReadOnlyList<IMemberInfo>>(7);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberPriority.Dynamic;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            if (name.Length == 0 || name[0] != FakeMemberPrefixSymbol && !name.StartsWith(FakeMemberPrefix, StringComparison.Ordinal))
                return Default.EmptyArray<IMemberInfo>();

            if (!_cache.TryGetValue(name, out var list))
            {
                list = new IMemberInfo[] {new ConstantMemberInfo(name, null, true)};
                _cache[name] = list;
            }

            return list;
        }

        #endregion
    }
}