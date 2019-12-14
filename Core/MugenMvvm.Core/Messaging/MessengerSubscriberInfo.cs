using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerSubscriberInfo
    {
        #region Fields

        public readonly ThreadExecutionMode ExecutionMode;
        public readonly object Subscriber;

        #endregion

        #region Constructors

        public MessengerSubscriberInfo(object subscriber, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
            Subscriber = subscriber;
            ExecutionMode = executionMode;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Subscriber == null;

        #endregion
    }
}