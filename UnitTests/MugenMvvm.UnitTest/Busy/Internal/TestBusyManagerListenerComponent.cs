using System;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Busy.Internal
{
    public sealed class TestBusyManagerListener : IBusyManagerListener
    {
        #region Properties

        public Action<IBusyManager, IBusyToken, IReadOnlyMetadataContext?>? OnBeginBusy { get; set; }

        public Action<IBusyManager, IReadOnlyMetadataContext?>? OnBusyChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            OnBeginBusy?.Invoke(busyManager, busyToken, metadata);
        }

        void IBusyManagerListener.OnBusyChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            OnBusyChanged?.Invoke(busyManager, metadata);
        }

        #endregion
    }
}