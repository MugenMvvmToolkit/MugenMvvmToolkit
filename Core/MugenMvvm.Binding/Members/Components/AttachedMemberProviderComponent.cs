using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedMemberProviderComponent : AttachableComponentBase<IMemberProvider>, IMemberProviderComponent
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedMemberProviderComponent()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo> TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            if (_cache.TryGetValue(new CacheKey(type, name), out var list))
                return list;
            return Default.EmptyArray<IMemberInfo>();
        }

        #endregion

        #region Methods

        public void Register(Type type, IMemberInfo member, string? name = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            var key = new CacheKey(type, name ?? member.Name);
            if (!_cache.TryGetValue(key, out var list))
            {
                list = new List<IMemberInfo>();
                _cache[key] = list;
            }

            if (member.MemberType != MemberType.Method)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].MemberType == member.MemberType)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }

            list.Add(member);
            (Owner as IHasCache)?.Invalidate(type);
        }

        public bool Unregister(Type type, MemberType memberType)
        {
            Should.NotBeNull(type, nameof(type));
            var removed = false;
            foreach (var keyValuePair in _cache)
            {
                if (keyValuePair.Key.Type != type)
                    continue;

                var list = keyValuePair.Value;
                for (var i = 0; i < list.Count; i++)
                {
                    if (memberType.HasFlagEx(list[i].MemberType))
                    {
                        list.RemoveAt(i--);
                        removed = true;
                    }
                }
            }

            if (removed)
                (Owner as IHasCache)?.Invalidate(type);
            return removed;
        }

        public bool Unregister(Type type, MemberType memberType, string name)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            if (!_cache.TryGetValue(new CacheKey(type, name), out var list))
                return false;

            var removed = false;
            for (var i = 0; i < list.Count; i++)
            {
                if (memberType.HasFlagEx(list[i].MemberType))
                {
                    list.RemoveAt(i--);
                    removed = true;
                }
            }

            if (removed)
                (Owner as IHasCache)?.Invalidate(type);
            return removed;
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionary<CacheKey, List<IMemberInfo>>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Name.Equals(y.Name) && x.Type == y.Type;
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return (key.Type.GetHashCode() * 397 ^ key.Name.GetHashCode()) * 397;
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

            #endregion

            #region Constructors

            public CacheKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            #endregion
        }

        #endregion
    }
}