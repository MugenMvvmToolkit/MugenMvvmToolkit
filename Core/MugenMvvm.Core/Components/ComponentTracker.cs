using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentTracker<T> : IComponentCollectionChangedListener where T : class
    {
        #region Fields

        private readonly IListener _listener;

        #endregion

        #region Constructors

        public ComponentTracker(IListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            _listener = listener;
        }

        #endregion

        #region Implementation of interfaces

        public void OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is T || component is IDecoratorComponentCollectionComponent<T>)
                _listener.OnComponentChanged(collection.Get<T>(metadata), metadata);
        }

        public void OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is T || component is IDecoratorComponentCollectionComponent<T>)
                _listener.OnComponentChanged(collection.Get<T>(metadata), metadata);
        }

        #endregion

        #region Methods

        public void Attach(IComponentOwner owner, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(owner, nameof(owner));
            Attach(owner.Components, metadata);
        }

        public void Attach(IComponentCollection collection, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.Add(this);
            collection.Components.Add(this);
            _listener.OnComponentChanged(collection.Get<T>(metadata), metadata);
        }

        public void Detach(IComponentOwner owner, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(owner, nameof(owner));
            Detach(owner.Components, metadata);
        }

        public void Detach(IComponentCollection collection, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.Add(this);
            collection.Components.Add(this);
            _listener.OnComponentChanged(Default.EmptyArray<T>(), metadata);
        }

        #endregion

        #region Nested types

        public interface IListener
        {
            void OnComponentChanged(T[] components, IReadOnlyMetadataContext? metadata);
        }

        #endregion
    }
}