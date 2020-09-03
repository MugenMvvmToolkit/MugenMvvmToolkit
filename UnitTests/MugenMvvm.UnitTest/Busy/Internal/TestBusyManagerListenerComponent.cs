using System;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Busy.Internal
{
    public sealed class TestBusyManagerListener : IBusyManagerListener
    {
        #region Fields

        private readonly IBusyManager? _owner;

        #endregion

        #region Constructors

        public TestBusyManagerListener(IBusyManager? owner = null)
        {
            _owner = owner;
        }

        #endregion

        #region Properties

        public Action<IBusyToken, IReadOnlyMetadataContext?>? OnBeginBusy { get; set; }

        public Action<IReadOnlyMetadataContext?>? OnBusyChanged { get; set; }

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}