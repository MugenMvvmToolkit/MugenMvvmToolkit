using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class ComponentOwnerBase<T> : IComponentOwner<T> where T : class
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        protected ComponentOwnerBase(IComponentCollectionProvider? componentCollectionProvider)
        {
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider => _componentCollectionProvider.DefaultIfNull();

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    ComponentCollectionProvider.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Methods

        protected TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            if (_components == null)
                return Default.EmptyArray<TComponent>();
            return _components.Get<TComponent>(metadata);
        }

        #endregion
    }
}