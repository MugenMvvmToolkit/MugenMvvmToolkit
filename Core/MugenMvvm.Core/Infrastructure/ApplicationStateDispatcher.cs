using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure
{
    public class ApplicationStateDispatcher : HasListenersBase<IApplicationStateDispatcherListener>, IApplicationStateDispatcher
    {
        #region Fields

        private ApplicationState _state;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ApplicationStateDispatcher()
            : this(ApplicationState.Active)
        {
        }

        public ApplicationStateDispatcher(ApplicationState state)
        {
            _state = state;
        }

        #endregion

        #region Properties

        public ApplicationState State => _state;

        #endregion

        #region Implementation of interfaces

        public void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(state, nameof(state));
            Should.NotBeNull(metadata, nameof(metadata));
            var oldState = Interlocked.Exchange(ref _state, state);
            if (oldState == state)
                return;
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnStateChanged(this, oldState, state, metadata);
        }

        #endregion
    }
}