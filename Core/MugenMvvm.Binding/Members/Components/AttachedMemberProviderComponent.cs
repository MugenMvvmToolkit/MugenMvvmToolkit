using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class AttachedMemberProviderComponent : AttachableComponentBase<IMemberProvider>, IMemberProviderComponent, IMethodProviderComponent
    {
        #region Fields

        private readonly CacheDictionary<IBindingMemberInfo> _cache;
        private readonly CacheDictionary<List<IBindingMethodInfo>> _methodsCache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedMemberProviderComponent()
        {
            _cache = new CacheDictionary<IBindingMemberInfo>();
            _methodsCache = new CacheDictionary<List<IBindingMethodInfo>>();
        }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            _cache.TryGetValue(new CacheKey(type, name), out var result);
            return result;
        }

        public IReadOnlyList<IBindingMethodInfo> TryGetMethods(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            _methodsCache.TryGetValue(new CacheKey(type, name), out var result);
            return (IReadOnlyList<IBindingMethodInfo>) result ?? Default.EmptyArray<IBindingMethodInfo>();
        }

        #endregion

        #region Methods

        public void Register(Type type, IBindingMemberInfo member, string? name = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            _cache[new CacheKey(type, name ?? member.Name)] = member;
            (Owner as IHasCache)?.ClearCache();
        }

        public bool Unregister(Type type, string? name = null)
        {
            return UnregisterInternal(_cache, type, name);
        }

        public void RegisterMethod(Type type, IBindingMethodInfo method, string? name = null)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(method, nameof(method));
            var key = new CacheKey(type, name ?? method.Name);
            if (!_methodsCache.TryGetValue(key, out var list))
            {
                list = new List<IBindingMethodInfo>();
                _methodsCache[key] = list;
            }

            list.Add(method);
            (Owner as IHasCache)?.ClearCache();
        }

        public bool UnregisterMethod(Type type, string? name = null)
        {
            return UnregisterInternal(_methodsCache, type, name);
        }

        private bool UnregisterInternal<T>(LightDictionary<CacheKey, T> cache, Type type, string? name) where T : class
        {
            Should.NotBeNull(type, nameof(type));
            if (name != null)
            {
                if (cache.Remove(new CacheKey(type, name)))
                {
                    (Owner as IHasCache)?.ClearCache();
                    return true;
                }

                return false;
            }

            List<CacheKey>? toRemove = null;
            foreach (var keyValuePair in cache)
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
                cache.Remove(toRemove[index]);
            (Owner as IHasCache)?.ClearCache();
            return true;
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary<TItem> : LightDictionary<CacheKey, TItem> where TItem : class
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