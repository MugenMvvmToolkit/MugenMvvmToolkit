using System;
using System.Collections.Generic;
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
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class CacheMemberProviderComponent : DecoratorComponentBase<IMemberProvider, IMemberProviderComponent>, IMemberProviderComponent, IHasPriority, IHasCache
    {
        #region Fields

        private readonly TempCacheDictionary<IMemberInfo?> _tempCache;
        private readonly TempCacheDictionary<IReadOnlyList<IMemberInfo>?> _tempMembersCache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheMemberProviderComponent()
        {
            _tempCache = new TempCacheDictionary<IMemberInfo?>();
            _tempMembersCache = new TempCacheDictionary<IReadOnlyList<IMemberInfo>?>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!Default.IsValueType<TState>() && state is Type type)
            {
                LazyList<CacheKey> keys = default;
                Invalidate(_tempCache, type, ref keys);
                keys.List?.Clear();
                Invalidate(_tempMembersCache, type, ref keys);
            }
            else
            {
                _tempCache.Clear();
                _tempMembersCache.Clear();
            }
        }

        public bool TryGetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IMemberInfo? member)
        {
            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (!_tempCache.TryGetValue(cacheKey, out member))
            {
                Components.TryGetMember(type, name, memberTypes, flags, metadata, out member);
                _tempCache[cacheKey] = member;
            }

            return true;
        }

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (!_tempMembersCache.TryGetValue(cacheKey, out var members))
            {
                members = Components.TryGetMembers(type, name, memberTypes, flags, metadata);
                _tempMembersCache[cacheKey] = members;
            }

            return members;
        }

        #endregion

        #region Methods

        protected override void DecorateInternal(IList<IMemberProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnAttachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnDetachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        private static void Invalidate<TItem>(LightDictionary<CacheKey, TItem> dictionary, Type type, ref LazyList<CacheKey> keys)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Key.Type == type)
                    keys.Add(pair.Key);
            }

            var list = keys.List;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    dictionary.Remove(list[i]);
            }
        }

        #endregion

        #region Nested types

        private sealed class TempCacheDictionary<TItem> : LightDictionary<CacheKey, TItem> where TItem : class?
        {
            #region Constructors

            public TempCacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.MemberType == y.MemberType && x.MemberFlags == y.MemberFlags && x.Name.Equals(y.Name) && x.Type == y.Type;
            }

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.Name, key.Type, (int)key.MemberType, (int)key.MemberFlags);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Name;
            public readonly Type Type;
            public readonly MemberType MemberType;
            public readonly MemberFlags MemberFlags;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name, MemberType memberType, MemberFlags memberFlags)
            {
                Type = type;
                if (name == null)
                    name = string.Empty;
                Name = name;
                MemberType = memberType;
                MemberFlags = memberFlags;
            }

            #endregion
        }

        #endregion
    }
}