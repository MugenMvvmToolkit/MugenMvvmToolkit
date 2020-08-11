using MugenMvvm.Interfaces.Requests;

namespace MugenMvvm.Requests
{
    public class CancelableRequest : ICancelableRequest
    {
        #region Constructors

        public CancelableRequest(bool? cancel = null, object? state = null)
        {
            State = state;
            Cancel = cancel;
        }

        #endregion

        #region Properties

        public object? State { get; set; }

        public bool? Cancel { get; set; }

        #endregion
    }
}