using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure
{
    public sealed class ApplicationStateDispatcher : IApplicationStateDispatcher
    {
        #region Fields

        private IComponentCollection<IApplicationStateDispatcherListener>? _listeners;
        private ApplicationState _state;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ApplicationStateDispatcher(IComponentCollection<IApplicationStateDispatcherListener>? listeners = null)
            : this(ApplicationState.Active, listeners)
        {
        }

        public ApplicationStateDispatcher(ApplicationState state, IComponentCollection<IApplicationStateDispatcherListener>? listeners = null)
        {
            Should.NotBeNull(state, nameof(state));
            _listeners = listeners;
            _state = state;
        }

        #endregion

        #region Properties

        public IComponentCollection<IApplicationStateDispatcherListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IApplicationStateDispatcherListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public ApplicationState State => _state;

        #endregion

        #region Methods

        public void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(state, nameof(state));
            Should.NotBeNull(metadata, nameof(metadata));
            var oldState = Interlocked.Exchange(ref _state, state);
            if (oldState == state)
                return;
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnStateChanged(this, oldState, state, metadata);
        }

        private IReadOnlyList<IApplicationStateDispatcherListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IApplicationStateDispatcherListener>();
        }

        #endregion
    }
}