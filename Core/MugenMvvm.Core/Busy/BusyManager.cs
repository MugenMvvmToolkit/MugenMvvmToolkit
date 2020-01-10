using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Busy
{
    public sealed class BusyManager : ComponentOwnerBase<IBusyManager>, IBusyManager
    {
        #region Constructors

        public BusyManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Properties

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        #endregion

        #region Implementation of interfaces

        public ActionToken Suspend()
        {
            return GetComponents<ISuspendable>().Suspend();
        }

        public IBusyToken BeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var token = GetComponents<IBusyManagerComponent>().TryBeginBusy(request, metadata);
            if (token == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return token;
        }

        public IBusyToken? TryGetToken<TState>(FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetToken(filter, state, metadata);
        }

        public IReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetTokens(metadata) ?? Default.EmptyArray<IBusyToken>();
        }

        #endregion
    }
}