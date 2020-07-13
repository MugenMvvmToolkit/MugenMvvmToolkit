namespace MugenMvvm.Busy
{
    public class DelayBusyRequest
    {
        #region Fields

        public readonly int Delay;
        public readonly object? Message;

        #endregion

        #region Constructors

        public DelayBusyRequest(object? message, int delay)
        {
            Message = message;
            Delay = delay;
        }

        #endregion
    }
}