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
    public sealed class MemberCache : ComponentCacheBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent
    {
        private readonly Dictionary<CacheKey, object?> _cache;

        [Preserve(Conditional = true)]
        public MemberCache(int priority = MemberComponentPriority.Cache)
            : base(priority)
        {
            _cache = new Dictionary<CacheKey, object?>(59);
        }

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

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata)
        {
            if (state is Type type)
            {
                var keys = new ItemOrListEditor<CacheKey>(2);
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

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Invalidate(component, metadata);

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Invalidate(component, metadata);

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct CacheKey : IEquatable<CacheKey>
        {
            public readonly string Key;
            public readonly Type Type;
            public readonly ushort MemberType;
            public readonly ushort MemberFlags;
            private readonly object? _typesRaw;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CacheKey(Type type, string key, EnumFlags<MemberType> memberType, EnumFlags<MemberFlags> memberFlags, ItemOrArray<Type> types)
            {
                Type = type;
                Key = key;
                MemberType = memberType.Value();
                MemberFlags = memberFlags.Value();
                _typesRaw = types.GetRawValue();
            }

            public ItemOrArray<Type> Types
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ItemOrArray.FromRawValue<Type>(_typesRaw);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(CacheKey other) =>
                MemberType == other.MemberType && MemberFlags == other.MemberFlags && Key.Equals(other.Key) && Type == other.Type &&
                InternalEqualityComparer.Equals(Types, other.Types);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => HashCode.Combine(Key, Type, MemberType, MemberFlags, Types.Count);
        }
    }
}