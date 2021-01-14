using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerSubscriberInfo
    {
        public readonly ThreadExecutionMode? ExecutionMode;
        public readonly object? Subscriber;

        public MessengerSubscriberInfo(object subscriber, ThreadExecutionMode? executionMode)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Subscriber = subscriber;
            ExecutionMode = executionMode;
        }

        public bool IsEmpty => Subscriber == null;
    }
}