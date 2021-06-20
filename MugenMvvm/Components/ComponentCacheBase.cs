using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Components
{
    public abstract class ComponentCacheBase<T, TComponent> : _CInternal<T, TComponent>, IHasCacheComponent<T>
        where T : class, IComponentOwner<T>
        where TComponent : class
    {
        protected ComponentCacheBase(int priority = ComponentPriority.Cache) : base(priority)
        {
        }

        void IHasCacheComponent<T>.Invalidate(T owner, object? state, IReadOnlyMetadataContext? metadata) => Invalidate(state, metadata);
    }

    // ReSharper disable once InconsistentNaming
    public abstract class _CInternal<T, TComponent> : ComponentDecoratorBase<T, TComponent>, IComponentCollectionChangedListener
        where T : class, IComponentOwner<T>
        where TComponent : class
    {
        internal _CInternal(int priority) : base(priority)
        {
        }

        protected abstract void Invalidate(object? state, IReadOnlyMetadataContext? metadata);

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent)
                Invalidate(component, metadata);
        }

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            Invalidate(owner, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Invalidate(owner, metadata);
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentAdded(collection, component, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentRemoved(collection, component, metadata);
    }
}