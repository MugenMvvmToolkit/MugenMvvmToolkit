using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Busy.Internal
{
    public class TestBusyToken : IBusyToken
    {
        #region Properties

        public bool IsSuspended { get; set; }

        public bool IsCompleted { get; set; }

        public object? Message { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null) => default;

        public ActionToken RegisterCallback(IBusyTokenCallback callback) => default;

        #endregion
    }
}