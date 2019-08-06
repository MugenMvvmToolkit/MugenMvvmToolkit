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
        public ApplicationStateDispatcher(IComponentCollectionProvider componentCollectionProvider)
            : this(ApplicationState.Active, componentCollectionProvider)
        {
        }

        public ApplicationStateDispatcher(ApplicationState state, IComponentCollectionProvider componentCollectionProvider)
            : base(componentCollectionProvider)
        {
            Should.NotBeNull(state, nameof(state));
            _state = state;
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
            {
                if (components[i] is IApplicationStateDispatcherListener listener)
                    listener.OnStateChanged(this, oldState, state, metadata);
            }
        }

        #endregion
    }
}