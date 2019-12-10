using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentTracker<T, TState> : IComponentCollectionChangedListener where T : class
    {
        #region Fields

        private readonly Action<TState, T[], IReadOnlyMetadataContext?> _listener;
        private readonly TState _state;

        #endregion

        #region Constructors

        public ComponentTracker(Action<TState, T[], IReadOnlyMetadataContext?> listener, TState state)
        {
            Should.NotBeNull(listener, nameof(listener));
            _listener = listener;
            _state = state;
        }

        #endregion

        #region Implementation of interfaces

        public void OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is T || component is IDecoratorComponentCollectionComponent<T>)
                _listener.Invoke(_state, collection.Get<T>(metadata), metadata);
        }

        public void OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is T || component is IDecoratorComponentCollectionComponent<T>)
                _listener.Invoke(_state, collection.Get<T>(metadata), metadata);
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
            _listener.Invoke(_state, collection.Get<T>(metadata), metadata);
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
            _listener.Invoke(_state, Default.EmptyArray<T>(), metadata);
        }

        #endregion
    }
}