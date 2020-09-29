using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class ComponentCacheBase<T, TComponent> : ComponentDecoratorBase<T, TComponent>, IComponentCollectionChangedListener, IHasCache
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Constructors

        protected ComponentCacheBase(int priority = ComponentPriority.Cache) : base(priority)
        {
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => OnComponentAdded(collection, component, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => OnComponentRemoved(collection, component, metadata);

        public abstract void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null);

        #endregion

        #region Methods

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            Invalidate(null, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Invalidate(null, metadata);
        }

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(null, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(null, metadata);
        }

        #endregion
    }
}