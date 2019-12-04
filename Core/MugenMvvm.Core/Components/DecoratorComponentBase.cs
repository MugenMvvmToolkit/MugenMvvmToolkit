using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class DecoratorComponentBase<T, TComponent> : AttachableComponentBase<T>, IDecoratorComponentCollectionComponent<TComponent>
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Fields

        protected TComponent[] Components;

        #endregion

        #region Constructors

        protected DecoratorComponentBase()
        {
            Components = Default.EmptyArray<TComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IDecoratorComponentCollectionComponent<TComponent>.Decorate(List<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorDecorate((TComponent)(object)this, Owner, components, ref Components);
            OnDecorated(metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.AddComponent(this, metadata);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this, metadata);
            Components = Default.EmptyArray<TComponent>();
        }

        protected virtual void OnDecorated(IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}