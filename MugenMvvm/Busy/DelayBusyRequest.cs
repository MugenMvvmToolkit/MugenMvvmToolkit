namespace MugenMvvm.Busy
{
    public class DelayBusyRequest
    {
        #region Constructors

        public DelayBusyRequest(object? message, int delay)
        {
            Message = message;
            Delay = delay;
        }

        #endregion

        #region Properties

        public int Delay { get; protected set; }

        public object? Message { get; protected set; }

        #endregion
    }
}