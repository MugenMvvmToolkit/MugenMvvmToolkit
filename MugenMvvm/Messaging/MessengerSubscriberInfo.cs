using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerSubscriberInfo : IEquatable<MessengerSubscriberInfo>
    {
        public readonly ThreadExecutionMode? ExecutionMode;
        public readonly object? Subscriber;

        public MessengerSubscriberInfo(object subscriber, ThreadExecutionMode? executionMode)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            Subscriber = subscriber;
            ExecutionMode = executionMode;
        }

        [MemberNotNullWhen(false, nameof(Subscriber), nameof(ExecutionMode))]
        public bool IsEmpty => Subscriber == null;

        public bool Equals(MessengerSubscriberInfo other) => Equals(ExecutionMode, other.ExecutionMode) && Equals(Subscriber, other.Subscriber);

        public override bool Equals(object? obj) => obj is MessengerSubscriberInfo other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ExecutionMode, Subscriber);
    }
}