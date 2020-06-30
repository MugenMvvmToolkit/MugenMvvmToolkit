using System;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Busy
{
    public sealed class BusyManager : ComponentOwnerBase<IBusyManager>, IBusyManager
    {
        #region Constructors

        public BusyManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Properties

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        #endregion

        #region Implementation of interfaces

        public ActionToken Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            return GetComponents<ISuspendable>().Suspend(state, metadata);
        }

        public IBusyToken? TryBeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryBeginBusy(request, metadata);
        }

        public IBusyToken? TryGetToken<TState>(in TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetToken(state, filter, metadata);
        }

        public ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> GetTokens(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetTokens(metadata);
        }

        #endregion
    }
}