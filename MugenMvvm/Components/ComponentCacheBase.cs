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
        protected ComponentCacheBase(int priority = ComponentPriority.Cache) : base(priority)
        {
        }

        public abstract void Invalidate(object sender, object? state = null, IReadOnlyMetadataContext? metadata = null);

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(collection, null, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(collection, null, metadata);
        }

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            Invalidate(owner, null, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Invalidate(owner, null, metadata);
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentAdded(collection, component, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentRemoved(collection, component, metadata);
    }
}