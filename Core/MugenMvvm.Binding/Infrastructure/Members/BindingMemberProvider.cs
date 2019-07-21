using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    public class BindingMemberProvider : ComponentOwnerBase<IBindingMemberProvider>, IBindingMemberProvider, IComponentOwnerAddedCallback<IComponent<IBindingMemberProvider>>
    {
        #region Fields

        protected readonly HashSet<string> CurrentNames;
        protected readonly TempCacheDictionary TempCache;

        private IAttachedBindingMemberProviderComponent? _attachedBindingMemberProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingMemberProvider(IComponentCollectionProvider componentCollectionProvider)
            : base(componentCollectionProvider)
        {
            TempCache = new TempCacheDictionary();
            CurrentNames = new HashSet<string>(StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        protected IAttachedBindingMemberProviderComponent AttachedBindingMemberProvider
        {
            get
            {
                Should.NotBeNull(_attachedBindingMemberProvider, nameof(AttachedBindingMemberProvider));
                return _attachedBindingMemberProvider;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMemberInternal(type, name, false, metadata);
        }

        public IBindingMemberInfo? GetRawMember(Type type, string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMemberInternal(type, name, true, metadata);
        }

        public IReadOnlyList<AttachedMemberRegistration> GetAttachedMembers(Type type, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetAttachedMembersInternal(type, metadata);
        }

        public void Register(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(metadata, nameof(metadata));
            RegisterInternal(type, member, name, metadata);
        }

        public bool Unregister(Type type, string? name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(metadata, nameof(metadata));
            return UnregisterInternal(type, name, metadata);
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingMemberProvider>>.OnComponentAdded(object collection, IComponent<IBindingMemberProvider> component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IAttachedBindingMemberProviderComponent provider)
            {
                if (ReferenceEquals(_attachedBindingMemberProvider, provider))
                    return;
                if (_attachedBindingMemberProvider != null)
                    Components.Remove(_attachedBindingMemberProvider, metadata);
                _attachedBindingMemberProvider = provider;
            }
        }

        #endregion

        #region Methods

        protected virtual IBindingMemberInfo? GetMemberInternal(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext metadata)
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

        protected virtual IReadOnlyList<AttachedMemberRegistration> GetAttachedMembersInternal(Type type, IReadOnlyMetadataContext metadata)
        {
            return AttachedBindingMemberProvider.GetMembers(type, metadata);
        }

        protected virtual void RegisterInternal(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext metadata)
        {
            AttachedBindingMemberProvider.Register(type, member, name, metadata);
            TempCache.Clear();
        }

        protected virtual bool UnregisterInternal(Type type, string? name, IReadOnlyMetadataContext metadata)
        {
            var result = AttachedBindingMemberProvider.Unregister(type, name, metadata);
            if (result)
                TempCache.Clear();
            return result;
        }

        protected IBindingMemberInfo? GetMemberImpl(Type type, string name, bool ignoreAttachedMembers, IReadOnlyMetadataContext metadata)
        {
            var cacheKey = new CacheKey(type, name, ignoreAttachedMembers);
            if (TempCache.TryGetValue(cacheKey, out var result))
                return result;

            if (!ignoreAttachedMembers)
            {
                result = AttachedBindingMemberProvider.TryGetMember(type, name, metadata);
                if (result != null)
                {
                    TempCache[cacheKey] = result;
                    return result;
                }
            }

            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                result = (items[i] as IBindingMemberProviderComponent)?.TryGetMember(this, type, name, metadata);
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