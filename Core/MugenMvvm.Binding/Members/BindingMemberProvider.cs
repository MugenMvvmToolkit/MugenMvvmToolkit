using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Metadata;
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
        protected readonly TempCacheDictionary<IBindingMemberInfo> TempCache;
        protected readonly TempCacheDictionary<IReadOnlyList<IBindingMethodInfo>> TempMethodsCache;

        protected IBindingMemberProviderComponent[] MemberProviders;
        protected IBindingMethodProviderComponent[] MethodProviders;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingMemberProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            MemberProviders = Default.EmptyArray<IBindingMemberProviderComponent>();
            MethodProviders = Default.EmptyArray<IBindingMethodProviderComponent>();
            TempCache = new TempCacheDictionary<IBindingMemberInfo>();
            TempMethodsCache = new TempCacheDictionary<IReadOnlyList<IBindingMethodInfo>>();
            CurrentNames = new HashSet<string>(StringComparer.Ordinal);
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            bool ignoreAttachedMembers = false;
            if (metadata != null)
                ignoreAttachedMembers = metadata.Get(BindingMemberMetadata.IgnoreAttachedMembers);
            return GetMemberInternal(type, name, ignoreAttachedMembers, metadata);
        }

        public IReadOnlyList<IBindingMethodInfo> GetMethods(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            bool ignoreAttachedMembers = false;
            if (metadata != null)
                ignoreAttachedMembers = metadata.Get(BindingMemberMetadata.IgnoreAttachedMembers);
            return GetMethodsInternal(type, name, ignoreAttachedMembers, metadata);
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
            MugenExtensions.ComponentTrackerOnAdded(ref MethodProviders, this, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IBindingMemberProvider>> collection, IComponent<IBindingMemberProvider> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref MemberProviders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref MethodProviders, collection, component, metadata);
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

        protected virtual IReadOnlyList<IBindingMethodInfo> GetMethodsInternal(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
        {
            return GetMethodsImpl(type, name, ignoreAttachedMembers, metadata);
        }

        protected virtual void ClearCacheInternal()
        {
            TempCache.Clear();
        }

        protected IBindingMemberInfo? GetMemberImpl(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new CacheKey(type, name, ignoreAttachedMembers);
            if (TempCache.TryGetValue(cacheKey, out var result))
                return result;

            for (var i = 0; i < MemberProviders.Length; i++)
            {
                result = MemberProviders[i].TryGetMember(type, name, metadata);
                if (result != null && (!ignoreAttachedMembers || !result.IsAttached))
                    break;
            }

            TempCache[cacheKey] = result;
            return result;
        }

        protected IReadOnlyList<IBindingMethodInfo> GetMethodsImpl(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext? metadata)
        {
            var cacheKey = new CacheKey(type, name, ignoreAttachedMembers);
            if (TempMethodsCache.TryGetValue(cacheKey, out var result))
                return result;

            List<IBindingMethodInfo>? methods = null;
            for (var i = 0; i < MethodProviders.Length; i++)
            {
                var m = MethodProviders[i].TryGetMethods(type, name, metadata);
                if (m == null || m.Count == 0)
                    continue;

                foreach (var info in m)
                {
                    if (ignoreAttachedMembers && info.IsAttached)
                        continue;

                    if (methods == null)
                        methods = new List<IBindingMethodInfo>();
                    methods.Add(info);
                }
            }

            TempMethodsCache[cacheKey] = (IReadOnlyList<IBindingMethodInfo>)methods ?? Default.EmptyArray<IBindingMethodInfo>();
            return result;
        }

        #endregion

        #region Nested types

        protected sealed class TempCacheDictionary<TItem> : LightDictionaryBase<CacheKey, TItem?> where TItem : class
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