using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedBindingMemberProviderComponent : IAttachedBindingMemberProviderComponent
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedBindingMemberProviderComponent()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            _cache.TryGetValue(new CacheKey(type, name), out var result);
            return result;
        }

        public IReadOnlyList<AttachedMemberRegistration> GetMembers(Type type, IReadOnlyMetadataContext? metadata)
        {
            var members = new List<AttachedMemberRegistration>();
            foreach (var member in _cache)
            {
                if (member.Key.Type.IsAssignableFromUnified(type))
                    members.Add(new AttachedMemberRegistration(member.Key.Name, member.Value));
            }

            return members;
        }

        #endregion

        #region Methods

        public void Register(Type type, IBindingMemberInfo member, string? name = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            _cache[new CacheKey(type, name ?? member.Name)] = member;
        }

        public bool Unregister(Type type, string? name = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            if (name != null)
                return _cache.Remove(new CacheKey(type, name));

            List<CacheKey>? toRemove = null;
            foreach (var keyValuePair in _cache)
            {
                if (keyValuePair.Key.Type != type)
                    continue;
                if (toRemove == null)
                    toRemove = new List<CacheKey>();
                toRemove.Add(keyValuePair.Key);
            }

            if (toRemove == null)
                return false;
            for (var index = 0; index < toRemove.Count; index++)
                _cache.Remove(toRemove[index]);
            return true;
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionaryBase<CacheKey, IBindingMemberInfo>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Name.Equals(y.Name) && x.Type.EqualsEx(y.Type);
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