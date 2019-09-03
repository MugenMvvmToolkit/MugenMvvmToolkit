using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App
{
    public sealed class ApplicationStateDispatcher : ComponentOwnerBase<IApplicationStateDispatcher>, IApplicationStateDispatcher
    {
        #region Fields

        private ApplicationState _state;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ApplicationStateDispatcher(ApplicationState? state = null, IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _state = state ?? ApplicationState.Active;
        }

        #endregion

        #region Properties

        public ApplicationState State => _state;

        #endregion

        #region Implementation of interfaces

        public void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(state, nameof(state));
            var oldState = Interlocked.Exchange(ref _state, state);
            if (oldState == state)
                return;
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IApplicationStateDispatcherListener)?.OnStateChanged(this, oldState, state, metadata);
        }

        #endregion
    }
}