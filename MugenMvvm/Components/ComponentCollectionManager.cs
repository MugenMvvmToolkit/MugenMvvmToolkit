using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollectionManager : IComponentCollectionManager
    {
        #region Fields

        private IComponentCollection? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ComponentCollectionManager()
        {
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    this.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IComponentCollection? TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(owner, nameof(owner));
            var result = GetComponentCollectionInternal(owner, metadata);
            _components.GetOrDefault<IComponentCollectionManagerListener>(metadata).OnComponentCollectionCreated(this, result, metadata);
            return result;
        }

        #endregion

        #region Methods

        private IComponentCollection GetComponentCollectionInternal(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (!ReferenceEquals(owner, this))
            {
                var collection = _components.GetOrDefault<IComponentCollectionProviderComponent>(metadata).TryGetComponentCollection(this, owner, metadata);
                if (collection != null)
                    return collection;
            }

            return new ComponentCollection(owner);
        }

        #endregion
    }
}