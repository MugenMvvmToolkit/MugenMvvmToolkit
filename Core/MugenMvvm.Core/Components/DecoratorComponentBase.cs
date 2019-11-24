using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class DecoratorComponentBase<T, TComponent> : ComponentTrackerBase<T, TComponent>
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.ComponentTrackerInitialize(out Components, this as TComponent);
            owner.Components.Components.Add(this);
        }

        protected override void OnComponentAdded(IComponentCollection<IComponent<T>> collection, IComponent<T> component, IReadOnlyMetadataContext? metadata)
        {
            if (MugenExtensions.TryGetComponentForDecorator(component, this as TComponent, collection.Owner) != null)
                base.OnComponentAdded(collection, component, metadata);
        }

        #endregion
    }
}