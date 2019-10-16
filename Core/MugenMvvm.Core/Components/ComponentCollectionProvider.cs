using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public class ComponentCollectionProvider : IComponentCollectionProvider
    {
        #region Fields

        private IComponentCollection<IComponent<IComponentCollectionProvider>>? _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ComponentCollectionProvider()
        {
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.HasItems;

        public IComponentCollection<IComponent<IComponentCollectionProvider>> Components
        {
            get
            {
                if (_components == null)
                    this.LazyInitialize(ref _components, this);
                return _components!;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            var result = GetComponentCollectionInternal<T>(owner, metadata);

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IComponentCollectionProviderComponent).Name);

            OnComponentCollectionCreated(result!, metadata);

            return result!;
        }

        #endregion

        #region Methods

        protected virtual IComponentCollection<T>? GetComponentCollectionInternal<T>(object owner, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (!ReferenceEquals(owner, this))
            {
                var collectionFactories = this.GetComponents();
                for (var i = 0; i < collectionFactories.Length; i++)
                {
                    var collection = (collectionFactories[i] as IComponentCollectionProviderComponent)?.TryGetComponentCollection<T>(owner, metadata);
                    if (collection != null)
                        return collection;
                }
            }

            return new ComponentCollection<T>(owner);
        }

        protected virtual void OnComponentCollectionCreated<T>(IComponentCollection<T> result, IReadOnlyMetadataContext? metadata)
            where T : class
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IComponentCollectionProviderListener)?.OnComponentCollectionCreated(this, result, metadata);
        }

        #endregion
    }
}