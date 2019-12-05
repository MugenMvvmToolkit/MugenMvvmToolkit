using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

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

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (state is Type type)
            {
                List<CacheKey>? keys = null;
                Invalidate(_tempCache, type, ref keys);
                keys?.Clear();
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
                var selectors = Components;
                for (var i = 0; i < selectors.Length; i++)
                {
                    if (selectors[i].TryGetMember(type, name, memberTypes, flags, metadata, out member))
                        break;
                }

                _tempCache[cacheKey] = member;
            }

            return true;
        }

        public bool TryGetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata, out IReadOnlyList<IMemberInfo>? members)
        {
            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (!_tempMembersCache.TryGetValue(cacheKey, out members))
            {
                var selectors = Components;
                for (var i = 0; i < selectors.Length; i++)
                {
                    if (selectors[i].TryGetMembers(type, name, memberTypes, flags, metadata, out members))
                        break;
                }

                _tempMembersCache[cacheKey] = members;
            }

            return members != null;
        }

        #endregion

        #region Methods

        protected override void Decorate(IList<IMemberProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.Decorate(components, metadata);
            Invalidate();
        }

        protected override void OnAttachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate();
        }

        protected override void OnDetachedInternal(IMemberProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate();
        }

        private static void Invalidate<TItem>(LightDictionary<CacheKey, TItem> dictionary, Type type, ref List<CacheKey>? keys)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Key.Type != type)
                    continue;
                if (keys == null)
                    keys = new List<CacheKey>();
                keys.Add(pair.Key);
            }

            if (keys == null || keys.Count == 0)
                return;
            for (var i = 0; i < keys.Count; i++)
                dictionary.Remove(keys[i]);
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