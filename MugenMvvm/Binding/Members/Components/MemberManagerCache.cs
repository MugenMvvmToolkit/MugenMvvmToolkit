using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class MemberManagerCache : ComponentCacheBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IHasPriority
    {
        #region Fields

        private readonly TempCacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberManagerCache()
        {
            _cache = new TempCacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            string? name;
            Type[]? types;
            if (TypeChecker.IsValueType<TRequest>())
            {
                if (typeof(TRequest) == typeof(MemberTypesRequest))
                {
                    var methodRequest = MugenExtensions.CastGeneric<TRequest, MemberTypesRequest>(request);
                    name = methodRequest.Name;
                    types = methodRequest.Types;
                }
                else
                {
                    name = null;
                    types = null;
                }
            }
            else
            {
                name = request as string;
                types = Default.Array<Type>();
            }

            if (name == null)
                return Components.TryGetMembers(type, memberTypes, flags, request, metadata);

            var cacheKey = new CacheKey(type, name, memberTypes, flags, types!);
            if (!_cache.TryGetValue(cacheKey, out var members))
            {
                members = Components.TryGetMembers(type, memberTypes, flags, request, metadata).GetRawValue();
                _cache[cacheKey] = members;
            }

            return ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(members);
        }

        #endregion

        #region Methods

        public override void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!TypeChecker.IsValueType<TState>() && state is Type type)
            {
                ItemOrList<CacheKey, List<CacheKey>> keys = default;
                foreach (var pair in _cache)
                {
                    if (pair.Key.Type == type)
                        keys.Add(pair.Key, key => key.Type == null);
                }

                var count = keys.Count(key => key.Type == null);
                for (var i = 0; i < count; i++)
                    _cache.Remove(keys.Get(i));
            }
            else
                _cache.Clear();
        }

        protected override void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Invalidate<object?>(null, metadata);
        }

        protected override void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            Invalidate<object?>(null, metadata);
        }

        #endregion

        #region Nested types

        private sealed class TempCacheDictionary : LightDictionary<CacheKey, object?>
        {
            #region Constructors

            public TempCacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
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

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.Key, key.Type, (int)key.MemberType, (int)key.MemberFlags, key.Types.Length);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
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