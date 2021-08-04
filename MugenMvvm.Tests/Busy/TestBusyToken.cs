using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Busy
{
    public class TestBusyToken : IBusyToken
    {
        public IBusyManager Owner { get; set; } = null!;

        public bool IsCompleted { get; set; }

        public object? Message { get; set; }

        public bool IsSuspended { get; set; }

        public ActionToken RegisterCallback(IBusyTokenCallback callback) => default;

        public void Dispose()
        {
        }

        public ActionToken Suspend(IReadOnlyMetadataContext? metadata = null) => default;
    }
}