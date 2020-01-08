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
    public sealed class CacheMemberPathProviderComponent : DecoratorComponentBase<IObserverProvider, IMemberPathProviderComponent>, IHasPriority, IMemberPathProviderComponent, IHasCache
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IMemberPath> _cache;

        #endregion

        #region Constructors

        public CacheMemberPathProviderComponent()
        {
            _cache = new StringOrdinalLightDictionary<IMemberPath>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

        public void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null)
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
                if (value == null)
                    return null;
                _cache[stringPath] = value;
            }

            return value;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            Invalidate();
        }

        protected override void OnDetachedInternal(IObserverProvider owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            Invalidate();
        }

        protected override void DecorateInternal(IList<IMemberPathProviderComponent> components, IReadOnlyMetadataContext? metadata)
        {
            base.DecorateInternal(components, metadata);
            Invalidate();
        }

        #endregion
    }
}