using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class MemberCache : ComponentCacheBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IEqualityComparer<MemberCache.CacheKey>
    {
        #region Fields

        private readonly Dictionary<CacheKey, object?> _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberCache(int priority = MemberComponentPriority.Cache)
            : base(priority)
        {
            _cache = new Dictionary<CacheKey, object?>(59, this);
        }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
        {
            return x.MemberType == y.MemberType && x.MemberFlags == y.MemberFlags && x.Key.Equals(y.Key) && x.Type == y.Type && InternalEqualityComparer.Equals(x.Types, y.Types);
        }

        int IEqualityComparer<CacheKey>.GetHashCode(CacheKey key) => HashCode.Combine(key.Key, key.Type, key.MemberType, key.MemberFlags, key.Types.Count);

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags,
            object request, IReadOnlyMetadataContext? metadata)
        {
            var name = request as string;
            ItemOrArray<Type> types;
            if (name == null && request is MemberTypesRequest r)
            {
                name = r.Name;
                types = r.Types;
            }
            else
                types = default;

            if (name == null)
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);

            var cacheKey = new CacheKey(type, name, memberTypes, flags, types);
            if (!_cache.TryGetValue(cacheKey, out var members))
            {
                members = Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata).GetRawValue();
                _cache[cacheKey] = members;
            }

            return ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(members);
        }

        #endregion

        #region Methods

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (state is Type type)
            {
                var keys = new ItemOrListEditor<CacheKey>();
                foreach (var pair in _cache)
                {
                    if (pair.Key.Type == type)
                        keys.Add(pair.Key);
                }

                var count = keys.Count;
                for (var i = 0; i < count; i++)
                    _cache.Remove(keys[i]);
            }
            else
                _cache.Clear();
        }

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Invalidate(null, metadata);

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Invalidate(null, metadata);

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct CacheKey
        {
            #region Fields

            public readonly string Key;
            public readonly Type Type;
            public readonly ushort MemberType;
            public readonly ushort MemberFlags;
            private readonly object? _typesRaw;

            #endregion

            #region Constructors

            public CacheKey(Type type, string key, EnumFlags<MemberType> memberType, EnumFlags<MemberFlags> memberFlags, ItemOrArray<Type> types)
            {
                Type = type;
                Key = key;
                MemberType = memberType.Value();
                MemberFlags = memberFlags.Value();
                _typesRaw = types.GetRawValue();
            }

            #endregion

            #region Properties

            public ItemOrArray<Type> Types
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ItemOrArray.FromRawValue<Type>(_typesRaw);
            }

            #endregion
        }

        #endregion
    }
}