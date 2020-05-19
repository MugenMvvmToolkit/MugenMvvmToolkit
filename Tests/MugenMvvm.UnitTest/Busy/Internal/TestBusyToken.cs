using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;

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

        public ActionToken Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            return default;
        }

        public ActionToken RegisterCallback(IBusyTokenCallback callback)
        {
            return default;
        }

        #endregion
    }
}