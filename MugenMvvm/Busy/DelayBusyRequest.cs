namespace MugenMvvm.Busy
{
    public class DelayBusyRequest
    {
        public DelayBusyRequest(object? message, int delay)
        {
            Message = message;
            Delay = delay;
        }

        public int Delay { get; protected set; }

        public object? Message { get; protected set; }
    }
}