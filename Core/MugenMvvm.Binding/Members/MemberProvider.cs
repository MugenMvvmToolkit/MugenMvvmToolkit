using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MemberProvider : ComponentOwnerBase<IMemberProvider>, IMemberProvider, IComponentOwnerAddedCallback<IComponent<IMemberProvider>>,
        IComponentOwnerRemovedCallback<IComponent<IMemberProvider>>, IHasCache
    {
        #region Fields

        private readonly TempCacheDictionary<IMemberInfo?> _tempCache;
        private readonly TempCacheDictionary<IReadOnlyList<IMemberInfo>> _tempMembersCache;

        private IMemberProviderComponent[] _memberProviders;
        private ISelectorMemberProviderComponent[] _memberSelectors;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _tempCache = new TempCacheDictionary<IMemberInfo?>();
            _tempMembersCache = new TempCacheDictionary<IReadOnlyList<IMemberInfo>>();
            _memberProviders = Default.EmptyArray<IMemberProviderComponent>();
            _memberSelectors = Default.EmptyArray<ISelectorMemberProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IMemberProvider>>.OnComponentAdded(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _memberProviders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _memberSelectors, collection, component);
            Invalidate();
        }

        void IComponentOwnerRemovedCallback<IComponent<IMemberProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IMemberProvider>> collection,
            IComponent<IMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberProviders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _memberSelectors, component);
            Invalidate();
        }

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

            var components = Components.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IHasCache)?.Invalidate(state, metadata);
        }

        public IMemberInfo? GetMember(Type type, string name, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;
            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (_tempCache.TryGetValue(cacheKey, out var result))
                return result;

            var selectors = _memberSelectors;
            for (var i = 0; i < selectors.Length; i++)
            {
                result = selectors[i].TryGetMember(type, name, memberTypes, flags, metadata);
                if (result != null)
                    break;
            }

            if (result == null)
            {
                var memberProviders = _memberProviders;
                for (var i = 0; i < memberProviders.Length; i++)
                {
                    var members = memberProviders[i].TryGetMembers(type, name, metadata);
                    if (members == null || members.Count == 0)
                        continue;

                    result = SelectMember(members, memberTypes, flags, metadata);
                    if (result != null)
                        break;
                }
            }

            _tempCache[cacheKey] = result;
            return result;
        }

        public IReadOnlyList<IMemberInfo> GetMembers(Type type, string name, MemberType memberTypes, MemberFlags flags,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (!flags.HasFlagEx(MemberFlags.NonPublic))
                flags |= MemberFlags.Public;
            var cacheKey = new CacheKey(type, name, memberTypes, flags);
            if (_tempMembersCache.TryGetValue(cacheKey, out var result))
                return result;

            List<IMemberInfo>? list = null;
            var memberProviders = _memberProviders;
            for (var i = 0; i < memberProviders.Length; i++)
            {
                var members = memberProviders[i].TryGetMembers(type, name, metadata);
                if (members == null || members.Count == 0)
                    continue;

                if (list == null)
                    list = new List<IMemberInfo>();

                for (var j = 0; j < members.Count; j++)
                {
                    var member = members[j];
                    if (memberTypes.HasFlagEx(member.MemberType) && flags.HasFlagEx(member.AccessModifiers))
                        list.Add(member);
                }
            }

            result = list ?? (IReadOnlyList<IMemberInfo>) Default.EmptyArray<IMemberInfo>();
            _tempMembersCache[cacheKey] = result;
            return result;
        }

        #endregion

        #region Methods

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

        private static IMemberInfo? SelectMember(IReadOnlyList<IMemberInfo> members, MemberType memberTypes, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (memberTypes.HasFlagEx(member.MemberType) && flags.HasFlagEx(member.AccessModifiers))
                    return member;
            }

            return null;
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
                unchecked
                {
                    var hashCode = key.Name.GetHashCode();
                    hashCode = hashCode * 397 ^ key.Type.GetHashCode();
                    hashCode = hashCode * 397 ^ key.MemberType.GetHashCode();
                    hashCode = hashCode * 397 ^ key.MemberFlags.GetHashCode();
                    return hashCode;
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Name;
            public readonly Type Type;
            public readonly byte MemberType;
            public readonly byte MemberFlags;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name, MemberType memberType, MemberFlags memberFlags)
            {
                Type = type;
                if (name == null)
                    name = string.Empty;
                Name = name;
                MemberType = (byte) memberType;
                MemberFlags = (byte) memberFlags;
            }

            #endregion
        }

        #endregion
    }
}