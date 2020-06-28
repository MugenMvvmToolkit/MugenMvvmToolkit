using System.ComponentModel;
using MugenMvvm.Interfaces.Requests;

namespace MugenMvvm.Requests
{
    public class CancelableRequest : CancelEventArgs, ICancelableRequest
    {
        #region Constructors

        public CancelableRequest(bool cancel, object? state = null)
            : base(cancel)
        {
            State = state;
        }

        #endregion

        #region Properties

        public object? State { get; set; }

        #endregion
    }
}