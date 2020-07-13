using System;
using System.Collections.Generic;
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

        public IBusyToken? TryBeginBusy(object? request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryBeginBusy(this, request, metadata);
        }

        public IBusyToken? TryGetToken(Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool> filter, object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetToken(this, filter, state, metadata);
        }

        public ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> GetTokens(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IBusyManagerComponent>().TryGetTokens(this, metadata);
        }

        #endregion
    }
}