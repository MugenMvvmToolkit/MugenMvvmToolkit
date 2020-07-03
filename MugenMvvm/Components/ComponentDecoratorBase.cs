using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public abstract class ComponentDecoratorBase<T, TComponent> : AttachableComponentBase<T>, IComponentCollectionDecorator<TComponent>
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Fields

        protected TComponent[] Components;

        #endregion

        #region Constructors

        protected ComponentDecoratorBase()
        {
            Components = Default.Array<TComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionDecorator<TComponent>.Decorate(IComponentCollection collection, IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            DecorateInternal(collection, components, metadata);
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
            Components = Default.Array<TComponent>();
        }

        protected virtual void DecorateInternal(IComponentCollection collection, IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            Components = this.Decorate(components);
        }

        #endregion
    }
}