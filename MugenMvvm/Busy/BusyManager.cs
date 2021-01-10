using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
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

        #region Implementation of interfaces

        public IBusyToken? TryBeginBusy(object? request, IReadOnlyMetadataContext? metadata = null) => GetComponents<IBusyManagerComponent>(metadata).TryBeginBusy(this, request, metadata);

        public IBusyToken? TryGetToken<TState>(TState state, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IBusyManagerComponent>(metadata).TryGetToken(this, filter, state, metadata);

        public ItemOrIReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null) => GetComponents<IBusyManagerComponent>(metadata).TryGetTokens(this, metadata);

        #endregion
    }
}