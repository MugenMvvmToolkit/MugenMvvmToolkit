using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class ComponentTrackerBase<T, TComponent> : AttachableComponentBase<T>, IComponentCollectionChangedListener<IComponent<T>>
        where TComponent : class
        where T : class, IComponentOwner<T>
    {
        #region Fields

        protected TComponent[] Components;

        #endregion

        #region Constructors

        protected ComponentTrackerBase()
        {
            Components = Default.EmptyArray<TComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<T>>.OnAdded(IComponentCollection<IComponent<T>> collection, IComponent<T> component, IReadOnlyMetadataContext? metadata)
        {
            if (!ReferenceEquals(component, this))
                MugenExtensions.ComponentTrackerOnAdded(ref Components, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<T>>.OnRemoved(IComponentCollection<IComponent<T>> collection, IComponent<T> component, IReadOnlyMetadataContext? metadata)
        {
            if (!ReferenceEquals(component, this))
                MugenExtensions.ComponentTrackerOnRemoved(ref Components, component);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.ComponentTrackerInitialize(out Components, this as TComponent);
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            Components = Default.EmptyArray<TComponent>();
        }

        #endregion
    }
}