using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
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
    public sealed class CacheMemberManagerDecorator : ComponentDecoratorBase<IMemberManager, IMemberManagerComponent>, IMemberManagerComponent, IHasPriority, IHasCache
    {
        #region Fields

        private readonly TempCacheDictionary<object?> _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public CacheMemberManagerDecorator()
        {
            _cache = new TempCacheDictionary<object?>();
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
                LazyList<MemberManagerRequest> keys = default;
                Invalidate(_cache, type, ref keys);
                keys.List?.Clear();
            }
            else
                _cache.Clear();
        }

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) == typeof(MemberManagerRequest))
            {
                var cacheKey = MugenExtensions.CastGeneric<TRequest, MemberManagerRequest>(request);
                if (!_cache.TryGetValue(cacheKey, out var rawValue))
                {
                    rawValue = Components.TryGetMembers(request, metadata).GetRawValue();
                    _cache[cacheKey] = rawValue;
                }

                return ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>>.FromRawValue(rawValue);
            }

            return default;
        }

        #endregion

        #region Methods

        protected override void DecorateInternal(IList<IMemberManagerComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnAttachedInternal(IMemberManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnDetachedInternal(IMemberManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        private static void Invalidate<TItem>(LightDictionary<MemberManagerRequest, TItem> dictionary, Type type, ref LazyList<MemberManagerRequest> keys)
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

        private sealed class TempCacheDictionary<TItem> : LightDictionary<MemberManagerRequest, TItem> where TItem : class?
        {
            #region Constructors

            public TempCacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(MemberManagerRequest x, MemberManagerRequest y)
            {
                return x.MemberTypes == y.MemberTypes && x.Flags == y.Flags && x.Name.Equals(y.Name) && x.Type == y.Type;
            }

            protected override int GetHashCode(MemberManagerRequest key)
            {
                return HashCode.Combine(key.Name, key.Type, (int) key.MemberTypes, (int) key.Flags);
            }

            #endregion
        }

        #endregion
    }
}