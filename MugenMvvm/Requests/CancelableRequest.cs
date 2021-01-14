using MugenMvvm.Interfaces.Requests;

namespace MugenMvvm.Requests
{
    public class CancelableRequest : ICancelableRequest
    {
        public CancelableRequest(bool? cancel = null, object? state = null)
        {
            State = state;
            Cancel = cancel;
        }

        public object? State { get; set; }

        public bool? Cancel { get; set; }
    }
}