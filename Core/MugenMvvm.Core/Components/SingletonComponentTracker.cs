using System.Linq;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public sealed class SingletonComponentTracker<TComponent, TComponentBase> : IComponentCollectionChangedListener<IComponent<TComponentBase>>,
            IHasService<TComponent>, IHasServiceOptional<TComponent>
        where TComponent : class
        where TComponentBase : class
    {
        #region Fields

        private readonly bool _autoDetachOld;
        private TComponent? _component;
        private IComponentOwner<TComponentBase>? _owner;

        #endregion

        #region Constructors

        public SingletonComponentTracker(bool autoDetachOld)
        {
            _autoDetachOld = autoDetachOld;
        }

        #endregion

        #region Properties

        TComponent IHasService<TComponent>.Service => GetComponent(true)!;

        TComponent? IHasServiceOptional<TComponent>.ServiceOptional => GetComponent(false);

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnAdded(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.SingletonComponentTrackerOnAdded(ref _component, _autoDetachOld, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnRemoved(IComponentCollection<IComponent<TComponentBase>> collection,
            IComponent<TComponentBase> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.SingletonComponentTrackerOnRemoved(ref _component, collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<TComponentBase>>.OnCleared(IComponentCollection<IComponent<TComponentBase>> collection,
            ItemOrList<IComponent<TComponentBase>?, IComponent<TComponentBase>[]> oldItems, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.SingletonComponentTrackerOnCleared(ref _component, collection, oldItems, metadata);
        }

        #endregion

        #region Methods

        public void Attach(IComponentOwner<TComponentBase> owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            _owner?.Components.Components.Remove(this);
            _owner = owner;
            _component = owner.Components.GetItems().OfType<TComponent>().FirstOrDefault();
            owner.Components.Components.Add(this);
        }

        public TComponent? GetComponent(bool throwIfNotInitialized)
        {
            if (throwIfNotInitialized && _component == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(TComponent).Name);
            return _component!;
        }

        #endregion
    }
}