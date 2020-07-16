using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation.Components
{
    public sealed class MemberPathProviderCache : ComponentCacheBase<IObservationManager, IMemberPathProviderComponent>, IMemberPathProviderComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, IMemberPath?> _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathProviderCache()
        {
            _cache = new Dictionary<string, IMemberPath?>(59, StringComparer.Ordinal);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath<TPath>(IObservationManager observationManager, in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TPath>() || !(path is string stringPath))
                return null;

            if (!_cache.TryGetValue(stringPath, out var value))
            {
                value = Components.TryGetMemberPath(observationManager, stringPath, metadata)!;
                _cache[stringPath] = value;
            }

            return value;
        }

        #endregion

        #region Methods

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            _cache.Clear();
        }

        #endregion
    }
}