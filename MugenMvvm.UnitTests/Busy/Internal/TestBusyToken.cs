using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Busy.Internal
{
    public class TestBusyToken : IBusyToken
    {
        public bool IsCompleted { get; set; }

        public object? Message { get; set; }

        public bool IsSuspended { get; set; }

        public ActionToken RegisterCallback(IBusyTokenCallback callback) => default;

        public void Dispose()
        {
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null) => default;
    }
}