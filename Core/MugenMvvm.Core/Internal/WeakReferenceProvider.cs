using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceProvider : ComponentOwnerBase<IWeakReferenceProvider>, IWeakReferenceProvider
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IWeakReferenceProviderComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public WeakReferenceProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IWeakReferenceProviderComponent, WeakReferenceProvider>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Methods

        public IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null)
        {
            if (item == null)
                return Default.WeakReference;
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            return _components!.TryGetWeakReference(item, metadata);
        }

        #endregion
    }
}