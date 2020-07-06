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
            Decorate(collection, components, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.AddComponent(this, metadata);
        }

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this, metadata);
            Components = Default.Array<TComponent>();
        }

        protected virtual void Decorate(IComponentCollection collection, IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            Components = this.Decorate(components);
        }

        #endregion
    }
}