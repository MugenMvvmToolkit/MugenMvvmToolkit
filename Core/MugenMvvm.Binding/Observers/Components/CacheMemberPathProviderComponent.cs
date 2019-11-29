﻿using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class CacheMemberPathProviderComponent : DecoratorComponentBase<IObserverProvider, IMemberPathProviderComponent>, IHasPriority, IMemberPathProviderComponent
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IMemberPath> _cache;
        private readonly FuncEx<string, IReadOnlyMetadataContext?, IMemberPath?> _getMemberPathStringDelegate;

        #endregion

        #region Constructors

        public CacheMemberPathProviderComponent()
        {
            _cache = new StringOrdinalLightDictionary<IMemberPath>(59);
            _getMemberPathStringDelegate = TryGetMemberPath;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (_getMemberPathStringDelegate is FuncEx<TPath, IReadOnlyMetadataContext?, IMemberPath?> provider)
                return provider.Invoke(path, metadata);
            return null;
        }

        #endregion

        #region Methods

        private IMemberPath? TryGetMemberPath(in string path, IReadOnlyMetadataContext? metadata)
        {
            if (!_cache.TryGetValue(path, out var value))
            {
                value = TryGetPath(path, metadata)!;
                if (value == null)
                    return null;
                _cache[path] = value;
            }

            return value;
        }

        private IMemberPath? TryGetPath(in string path, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < Components.Length; i++)
            {
                var memberPath = Components[i].TryGetMemberPath(path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        #endregion
    }
}