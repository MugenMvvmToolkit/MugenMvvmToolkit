using System;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Busy.Internal
{
    public sealed class TestBusyManagerListener : IBusyManagerListener
    {
        private readonly IBusyManager? _owner;

        public TestBusyManagerListener(IBusyManager? owner = null)
        {
            _owner = owner;
        }

        public Action<IBusyToken, IReadOnlyMetadataContext?>? OnBeginBusy { get; set; }

        public Action<IReadOnlyMetadataContext?>? OnBusyChanged { get; set; }

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            OnBeginBusy?.Invoke(busyToken, metadata);
        }

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            OnBusyChanged?.Invoke(metadata);
        }
    }
}