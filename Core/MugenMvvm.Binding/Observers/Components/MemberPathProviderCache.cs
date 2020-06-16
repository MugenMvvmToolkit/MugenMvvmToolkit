using MugenMvvm.Attributes;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathProviderCache : ComponentCacheBase<IObserverProvider, IMemberPathProviderComponent>, IMemberPathProviderComponent, IHasPriority
    {
        #region Fields

        private readonly StringOrdinalLightDictionary<IMemberPath?> _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathProviderCache()
        {
            _cache = new StringOrdinalLightDictionary<IMemberPath?>(59);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Cache;

        #endregion

        #region Implementation of interfaces

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

        public override void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            _cache.Clear();
        }

        #endregion
    }
}