using System.Collections.Generic;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class CacheMemberPathProviderDecorator : ComponentDecoratorBase<IObserverProvider, IMemberPathProviderComponent>, IHasPriority, IMemberPathProviderComponent, IHasCache
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IMemberPath?> _cache;

        #endregion

        #region Constructors

        public CacheMemberPathProviderDecorator()
        {
            _cache = new StringOrdinalLightDictionary<IMemberPath?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            _cache.Clear();
        }

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TPath>() || !(path is string stringPath))
                return null;

            if (!_cache.TryGetValue(stringPath, out var value))
            {
                value = Components.TryGetMemberPath(stringPath, metadata)!;
                _cache[stringPath] = value;
            }

            return value;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void OnDetachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate<object?>(null, metadata);
        }

        protected override void DecorateInternal(IList<IMemberPathProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate<object?>(null, metadata);
        }

        #endregion
    }
}