using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Components
{
    public sealed class SingletonComponentTracker<TComponent, TComponentBase> : IComponentCollectionChangedListener<IComponent<TComponentBase>>, IHasService<TComponent>
        where TComponent : class, IComponent
        where TComponentBase : class
    {
        #region Fields

        private IComponentOwner<TComponentBase>? _owner;

        #endregion

        #region Properties

        public TComponent Component { get; private set; }

        TComponent IHasService<TComponent>.Service
        {
            get
            {
                if (Component == null)
                    ExceptionManager.ThrowObjectNotInitialized(typeof(TComponent).Name);
                return Component!;
            }
        }

        #endregion

        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return 0;
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnAdded(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            if (component is TComponent c)
                Component = c;
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnRemoved(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(Component, component))
                Component = null;
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnCleared(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase>[] oldItems, IReadOnlyMetadataContext? metadata)
        {
            for (var index = 0; index < oldItems.Length; index++)
            {
                if (ReferenceEquals(Component, oldItems[index]))
                {
                    Component = null;
                    break;
                }
            }
        }

        #endregion

        #region Methods

        public void Attach(IComponentOwner<TComponentBase> owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            _owner?.Components.Components.Remove(this);
            _owner = owner;
            Component = owner.Components.GetItems().OfType<TComponent>().FirstOrDefault();
            owner.Components.Components.Add(this);
        }

        #endregion
    }
}