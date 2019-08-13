using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public class BindingMemberProvider : ComponentOwnerBase<IBindingMemberProvider>, IBindingMemberProvider, IComponentOwnerAddedCallback<IComponent<IBindingMemberProvider>>,
        IComponentOwnerRemovedCallback<IComponent<IBindingMemberProvider>>, IHasCache
    {
        #region Fields

        protected readonly HashSet<string> CurrentNames;
        protected readonly TempCacheDictionary TempCache;

        protected IAttachedBindingMemberProviderComponent[] AttachedProviders;
        protected IBindingMemberProviderComponent[] MemberProviders;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingMemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            AttachedProviders = Default.EmptyArray<IAttachedBindingMemberProviderComponent>();
            MemberProviders = Default.EmptyArray<IBindingMemberProviderComponent>();
            TempCache = new TempCacheDictionary();
            CurrentNames = new HashSet<string>(StringComparer.Ordinal);
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            return GetMemberInternal(type, name, false, metadata);
        }

        public IBindingMemberInfo? GetRawMember(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            return GetMemberInternal(type, name, true, metadata);
        }

        public IReadOnlyList<AttachedMemberRegistration> GetAttachedMembers(Type type, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            return GetAttachedMembersInternal(type, metadata);
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingMemberProvider>>.OnComponentAdded(IComponentCollection<IComponent<IBindingMemberProvider>> collection,
            IComponent<IBindingMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IBindingMemberProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IBindingMemberProvider>> collection,
            IComponent<IBindingMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        public void ClearCache()
        {
            ClearCacheInternal();
        }

        #endregion

        #region Methods

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IBindingMemberProvider>> collection, IComponent<IBindingMemberProvider> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref MemberProviders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref AttachedProviders, this, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IBindingMemberProvider>> collection, IComponent<IBindingMemberProvider> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref MemberProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref AttachedProviders, collection, component, metadata);
        }

        protected virtual IBindingMemberInfo? GetMemberInternal(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (!CurrentNames.Add(name))
                    return null;

                return GetMemberImpl(type, name, ignoreAttachedMembers, metadata);
            }
            finally
            {
                CurrentNames.Remove(name);
            }
        }

        protected virtual void ClearCacheInternal()
        {
            TempCache.Clear();
        }

        protected virtual IReadOnlyList<AttachedMemberRegistration> GetAttachedMembersInternal(Type type, IReadOnlyMetadataContext? metadata)
        {
            if (AttachedProviders.Length == 1)
                return AttachedProviders[0].GetMembers(type, metadata);

            List<AttachedMemberRegistration>? result = null;
            for (var i = 0; i < AttachedProviders.Length; i++)
            {
                var list = AttachedProviders[i].GetMembers(type, metadata);
                if (list != null && list.Count != 0)
                {
                    if (result == null)
                        result = new List<AttachedMemberRegistration>();
                    result.AddRange(list);
                }
            }

            return result ?? (IReadOnlyList<AttachedMemberRegistration>) Default.EmptyArray<AttachedMemberRegistration>();
        }

        protected virtual IBindingMemberInfo? TryGetAttachedMember(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < AttachedProviders.Length; i++)
            {
                var member = AttachedProviders[i].TryGetMember(type, name, metadata);
                if (member != null)
                    return member;
            }

            return null;
        }

        protected IBindingMemberInfo? GetMemberImpl(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new CacheKey(type, name, ignoreAttachedMembers);
            if (TempCache.TryGetValue(cacheKey, out var result))
                return result;

            if (!ignoreAttachedMembers)
            {
                result = TryGetAttachedMember(type, name, metadata);
                if (result != null)
                {
                    TempCache[cacheKey] = result;
                    return result;
                }
            }

            for (var i = 0; i < MemberProviders.Length; i++)
            {
                result = MemberProviders[i].TryGetMember(this, type, name, metadata);
                if (result != null)
                    break;
            }

            TempCache[cacheKey] = result;
            return result;
        }

        #endregion

        #region Nested types

        protected sealed class TempCacheDictionary : LightDictionaryBase<CacheKey, IBindingMemberInfo?>
        {
            #region Constructors

            public TempCacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.IgnoreAttachedMembers == y.IgnoreAttachedMembers &&
                       x.Name.Equals(y.Name) && x.Type.EqualsEx(y.Type);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return (key.Type.GetHashCode() * 397 ^ key.Name.GetHashCode()) * 397 ^ key.IgnoreAttachedMembers.GetHashCode();
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        protected readonly struct CacheKey
        {
            #region Fields

            public readonly string Name;
            public readonly Type Type;
            public readonly bool IgnoreAttachedMembers;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name, bool ignoreAttachedMembers)
            {
                Type = type;
                if (name == null)
                    name = string.Empty;
                Name = name;
                IgnoreAttachedMembers = ignoreAttachedMembers;
            }

            #endregion
        }

        #endregion
    }
}