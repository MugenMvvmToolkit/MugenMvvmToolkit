using System;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Busy
{
    public sealed class TestBusyManagerListener : IBusyManagerListener
    {
        public Action<IBusyManager, IBusyToken, IReadOnlyMetadataContext?>? OnBeginBusy { get; set; }

        public Action<IBusyManager, IReadOnlyMetadataContext?>? OnBusyChanged { get; set; }

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata) =>
            OnBeginBusy?.Invoke(busyManager, busyToken, metadata);

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) => OnBusyChanged?.Invoke(busyManager, metadata);
    }
}