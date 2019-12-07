using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class DecoratorComponentBase<T, TComponent> : AttachableComponentBase<T>, IDecoratorComponentCollectionComponent<TComponent>, IComparer<object>
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

        int IComparer<object>.Compare(object x, object y)
        {
            return MugenExtensions.GetComponentPriority(y, Owner).CompareTo(MugenExtensions.GetComponentPriority(x, Owner));
        }

        void IDecoratorComponentCollectionComponent<TComponent>.Decorate(IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            Decorate(components, metadata);
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

        protected virtual void Decorate(IList<TComponent> components, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentDecoratorDecorate((TComponent)(object)this, Owner, components, this, ref Components);
        }

        #endregion
    }
}