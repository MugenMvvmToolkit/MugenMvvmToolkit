using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MemberManagerCache : ComponentCacheBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IHasPriority, IEqualityComparer<MemberManagerCache.CacheKey>
    {
        #region Fields

        private readonly Dictionary<CacheKey, object?> _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberManagerCache()
        {
            _cache = new Dictionary<CacheKey, object?>(59, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
        {
            if (x.MemberType != y.MemberType || x.MemberFlags != y.MemberFlags || !x.Key.Equals(y.Key) || x.Type != y.Type || x.Types.Length != y.Types.Length)
                return false;
            if (ReferenceEquals(x.Types, y.Types))
                return true;
            for (var i = 0; i < x.Types.Length; i++)
            {
                if (x.Types[i] != y.Types[i])
                    return false;
            }

            return true;
        }

        int IEqualityComparer<CacheKey>.GetHashCode(CacheKey key)
        {
            return HashCode.Combine(key.Key, key.Type, (int)key.MemberType, (int)key.MemberFlags, key.Types.Length);
        }

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, MemberType memberTypes, MemberFlags flags, object request, IReadOnlyMetadataContext? metadata)
        {
            var name = request as string;
            Type[]? types;
            if (name == null && request is MemberTypesRequest r)
            {
                name = r.Name;
                types = r.Types;
            }
            else
                types = Default.Array<Type>();

            if (name == null)
                return Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata);

            var cacheKey = new CacheKey(type, name, memberTypes, flags, types!);
            if (!_cache.TryGetValue(cacheKey, out var members))
            {
                members = Components.TryGetMembers(memberManager, type, memberTypes, flags, request, metadata).GetRawValue();
                _cache[cacheKey] = members;
            }

            return ItemOrList.FromRawValueReadonly<IMemberInfo>(members, true);
        }

        #endregion

        #region Methods

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (state is Type type)
            {
                var keys = ItemOrListEditor.Get<CacheKey>(key => key.Type == null);
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

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Invalidate(null, metadata);
        }

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Invalidate(null, metadata);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct CacheKey
        {
            #region Fields

            public readonly string Key;
            public readonly Type Type;
            public readonly MemberType MemberType;
            public readonly MemberFlags MemberFlags;
            public readonly Type[] Types;

            #endregion

            #region Constructors

            public CacheKey(Type type, string key, MemberType memberType, MemberFlags memberFlags, Type[] types)
            {
                Type = type;
                Key = key;
                MemberType = memberType;
                MemberFlags = memberFlags;
                Types = types;
            }

            #endregion
        }

        #endregion
    }
}