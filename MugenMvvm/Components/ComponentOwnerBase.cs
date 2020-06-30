using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public abstract class ComponentOwnerBase<T> : IComponentOwner<T> where T : class
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private IComponentCollection? _components;

        #endregion

        #region Constructors

        protected ComponentOwnerBase(IComponentCollectionManager? componentCollectionManager)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        protected IComponentCollectionManager ComponentCollectionManager => _componentCollectionManager.DefaultIfNull();

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    ComponentCollectionManager.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion

        #region Methods

        protected TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            if (_components == null)
                return Default.Array<TComponent>();
            return _components.Get<TComponent>(metadata);
        }

        #endregion
    }
}