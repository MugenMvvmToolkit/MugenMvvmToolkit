using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Busy;

namespace MugenMvvm.Busy
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct DelayBusyRequest
    {
        #region Fields

        public readonly object? Message;
        public readonly int MillisecondsDelay;

        #endregion

        #region Constructors

        public DelayBusyRequest(object? message, int millisecondsDelay)
        {
            Message = message;
            MillisecondsDelay = millisecondsDelay;
        }

        #endregion

        #region Properties

        public IBusyToken? ParentToken => Message as IBusyToken;

        #endregion
    }
}