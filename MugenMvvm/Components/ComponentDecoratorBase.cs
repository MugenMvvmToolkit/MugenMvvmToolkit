using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Components
{
    public abstract class ComponentDecoratorBase<T, TComponent> : AttachableComponentBase<T>, IComponentCollectionDecorator<TComponent>, IHasPriority
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        protected ItemOrArray<TComponent> Components;

        protected ComponentDecoratorBase(int priority = ComponentPriority.Decorator)
        {
            Priority = priority;
        }

        public int Priority { get; protected set; }

        protected virtual void Decorate(IComponentCollection collection, ref ItemOrListEditor<TComponent> components, IReadOnlyMetadataContext? metadata) =>
            Components = this.Decorate(ref components);

        protected override void OnAttached(T owner, IReadOnlyMetadataContext? metadata) => owner.Components.AddComponent(this, metadata);

        protected override void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.RemoveComponent(this, metadata);
            Components = default;
        }

        void IComponentCollectionDecorator<TComponent>.Decorate(IComponentCollection collection, ref ItemOrListEditor<TComponent> components, IReadOnlyMetadataContext? metadata) =>
            Decorate(collection, ref components, metadata);
    }
}