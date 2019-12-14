using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedValueManager : ComponentOwnerBase<IAttachedValueManager>, IAttachedValueManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IAttachedValueManagerComponent[]? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedValueManager(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IAttachedValueManagerComponent, AttachedValueManager>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedValueProvider GetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            var provider = _components!.TryGetOrAddAttachedValueProvider(item, metadata);
            if (provider == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return provider;
        }

        public IAttachedValueProvider? GetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            if (_components == null)
                _componentTracker.Attach(this, metadata);
            _components!.TryGetAttachedValueProvider(item, metadata, out var provider);
            return provider;
        }

        #endregion
    }
}