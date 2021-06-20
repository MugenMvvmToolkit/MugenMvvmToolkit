using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Components
{
    public sealed class ComponentTracker : IComponentCollectionChangedListener, IHasCacheComponent<IComponentCollection>
    {
        private int _flag;
        private Listener _listener;
        private List<Listener>? _listeners;

        public void AddListener<T, TState>(Action<ItemOrArray<T>, TState, IReadOnlyMetadataContext?> listener, TState state)
            where T : class
            where TState : class
        {
            var l = new Listener(listener, state, o => o is T || o is IComponentCollectionDecorator<T>, (b, del, s, collection, metadata) =>
            {
                var action = (Action<ItemOrArray<T>, TState, IReadOnlyMetadataContext?>)del;
                action.Invoke(b ? collection.Get<T>() : default, (TState)s!, metadata);
            });
            if (_listeners != null)
            {
                _listeners.Add(l);
                return;
            }

            if (_listener.IsEmpty)
                _listener = l;
            else
            {
                _listeners = new List<Listener>(2) { _listener, l };
                _listener = default;
            }
        }

        public void Attach(IComponentOwner owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(owner, nameof(owner));
            Attach(owner.Components, metadata);
        }

        public void Attach(IComponentCollection collection, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (Interlocked.CompareExchange(ref _flag, int.MaxValue, 0) != 0)
                return;
            collection.Components.Add(this, metadata);
            if (_listeners != null)
            {
                for (var i = 0; i < _listeners.Count; i++)
                    _listeners[i].Update(collection, metadata);
            }
            else if (!_listener.IsEmpty)
                _listener.Update(collection, metadata);
        }

        public void Detach(IComponentOwner owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(owner, nameof(owner));
            Detach(owner.Components, metadata);
        }

        public void Detach(IComponentCollection collection, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (Interlocked.CompareExchange(ref _flag, 0, int.MaxValue) != int.MaxValue)
                return;
            collection.Components.Remove(this, metadata);
            if (_listeners != null)
            {
                for (var i = 0; i < _listeners.Count; i++)
                    _listeners[i].Clear(collection, metadata);
            }
            else if (!_listener.IsEmpty)
                _listener.Clear(collection, metadata);
        }

        public void OnComponentChanged(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (_listeners != null)
            {
                for (var i = 0; i < _listeners.Count; i++)
                    _listeners[i].OnComponentChanged(component, collection, metadata);
            }
            else if (!_listener.IsEmpty)
                _listener.OnComponentChanged(component, collection, metadata);
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentChanged(collection, component, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnComponentChanged(collection, component, metadata);

        void IHasCacheComponent<IComponentCollection>.Invalidate(IComponentCollection owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (state != null)
                OnComponentChanged(owner, state, metadata);
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct Listener
        {
            private readonly Delegate? _listener;
            private readonly object? _state;
            private readonly Action<bool, Delegate, object?, IComponentCollection, IReadOnlyMetadataContext?>? _update;
            private readonly Func<object, bool>? IsValidComponent;

            public Listener(Delegate listener, object? state, Func<object, bool> isValidComponent,
                Action<bool, Delegate, object?, IComponentCollection, IReadOnlyMetadataContext?> update)
            {
                Should.NotBeNull(listener, nameof(listener));
                _listener = listener;
                _state = state;
                IsValidComponent = isValidComponent;
                _update = update;
            }

            public bool IsEmpty => _listener == null;

            public void OnComponentChanged(object component, IComponentCollection collection, IReadOnlyMetadataContext? metadata)
            {
                if (IsValidComponent!(component))
                    Update(collection, metadata);
            }

            public void Update(IComponentCollection collection, IReadOnlyMetadataContext? metadata) => _update!(true, _listener!, _state, collection, metadata);

            public void Clear(IComponentCollection collection, IReadOnlyMetadataContext? metadata) => _update!(false, _listener!, _state, collection, metadata);
        }
    }
}