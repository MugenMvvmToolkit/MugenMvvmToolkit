using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class DecoratorTrackerComponentBase<T, TComponent> : AttachableComponentBase<T>, IComponentCollectionChangedListener
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Fields

        protected TComponent[] Components;

        #endregion

        #region Constructors

        protected DecoratorTrackerComponentBase()
        {
            Components = Default.EmptyArray<TComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            TComponent[]? _ = null;
            MugenExtensions.ComponentDecoratorInitialize(this, owner, metadata, ref _, ref Components);
            owner.Components.AddComponent(this);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this);
            Components = Default.EmptyArray<TComponent>();
        }

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            TComponent[]? _ = null;
            MugenExtensions.ComponentDecoratorOnAdded(this, collection, component, ref _, ref Components);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            TComponent[]? _ = null;
            MugenExtensions.ComponentDecoratorOnRemoved(this, component, ref _, ref Components);
        }

        #endregion
    }
}