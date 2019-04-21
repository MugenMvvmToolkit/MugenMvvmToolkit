using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.App
{
    public sealed class ApplicationStateDispatcher : IApplicationStateDispatcher
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;

        private IComponentCollection<IApplicationStateDispatcherListener>? _listeners;
        private ApplicationState _state;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ApplicationStateDispatcher(IComponentCollectionProvider? componentCollectionProvider = null)
            : this(ApplicationState.Active, componentCollectionProvider)
        {
        }

        public ApplicationStateDispatcher(ApplicationState state, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            Should.NotBeNull(state, nameof(state));
            _state = state;
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IApplicationStateDispatcherListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this, _componentCollectionProvider);
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
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnStateChanged(this, oldState, state, metadata);
        }

        private IApplicationStateDispatcherListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}