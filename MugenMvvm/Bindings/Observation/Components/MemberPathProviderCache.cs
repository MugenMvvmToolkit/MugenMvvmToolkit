using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class MemberPathProviderCache : ComponentCacheBase<IObservationManager, IMemberPathProviderComponent>, IMemberPathProviderComponent
    {
        private readonly Dictionary<string, IMemberPath?> _cache;

        [Preserve(Conditional = true)]
        public MemberPathProviderCache(int priority = ObserverComponentPriority.Cache)
            : base(priority)
        {
            _cache = new Dictionary<string, IMemberPath?>(59, StringComparer.Ordinal);
        }

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null) => _cache.Clear();

        public IMemberPath? TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            if (!(path is string stringPath))
                return null;

            if (!_cache.TryGetValue(stringPath, out var value))
            {
                value = Components.TryGetMemberPath(observationManager, stringPath, metadata)!;
                _cache[stringPath] = value;
            }

            return value;
        }
    }
}