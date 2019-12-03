using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class DecoratorComponentBase<T, TComponent> : AttachableComponentBase<T>, IComponentCollectionChangedListener, IDecoratorComponentCollectionComponent<TComponent>
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Fields

        private TComponent[] _decoratorComponents;
        protected TComponent[] Components;

        #endregion

        #region Constructors

        protected DecoratorComponentBase()
        {
            Components = Default.EmptyArray<TComponent>();
            _decoratorComponents = Components;
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

        bool IDecoratorComponentCollectionComponent<TComponent>.TryDecorate(ref TComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            return TryDecorate(ref components, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorInitialize(this, owner, metadata, ref _decoratorComponents, ref Components);
            owner.Components.AddComponent(this, metadata);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this, metadata);
            Components = Default.EmptyArray<TComponent>();
            _decoratorComponents = Components;
        }

        protected virtual void OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorOnAdded(this, collection, component, ref _decoratorComponents, ref Components);
        }

        protected virtual void OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorOnRemoved(this, component, ref _decoratorComponents, ref Components);
        }

        protected virtual bool TryDecorate(ref TComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            components = _decoratorComponents;
            return true;
        }

        #endregion
    }
}