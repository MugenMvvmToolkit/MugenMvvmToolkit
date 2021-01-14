using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class ResourceMemberPathObserverCache : ComponentCacheBase<IObservationManager, IMemberPathObserverProviderComponent>, IMemberPathObserverProviderComponent
    {
        private readonly Dictionary<string, IMemberPathObserver?> _cache;

        public ResourceMemberPathObserverCache(int priority = ObserverComponentPriority.Cache) : base(priority)
        {
            _cache = new Dictionary<string, IMemberPathObserver?>(3, StringComparer.Ordinal);
        }

        public override void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (state is string key)
            {
#if NET_CORE
                _cache.Remove(key, out var v);
#else
                if (_cache.TryGetValue(key, out var v))
                    _cache.Remove(key);
#endif
                if (v != null)
                    v.IsDisposable = true;
            }
            else
            {
                foreach (var observer in _cache)
                    if (observer.Value != null)
                        observer.Value.IsDisposable = true;

                _cache.Clear();
            }
        }

        public IMemberPathObserver? TryGetMemberPathObserver(IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is MemberPathObserverRequest {Expression: BindingResourceMemberExpressionNode exp} r && r.Path.Path == nameof(IDynamicResource.Value))
            {
                if (!_cache.TryGetValue(exp.ResourceName, out var v))
                {
                    v = Components.TryGetMemberPathObserver(observationManager, target, request, metadata);
                    if (v != null)
                        v.IsDisposable = false;
                    _cache[exp.ResourceName] = v;
                }

                return v;
            }

            return Components.TryGetMemberPathObserver(observationManager, target, request, metadata);
        }
    }
}