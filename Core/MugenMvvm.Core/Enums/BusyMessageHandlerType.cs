using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class BusyMessageHandlerType : EnumBase<BusyMessageHandlerType, int>
    {
        #region Fields

        public static readonly BusyMessageHandlerType None = new BusyMessageHandlerType(0);
        public static readonly BusyMessageHandlerType Handle = new BusyMessageHandlerType(1);
        public static readonly BusyMessageHandlerType NotifySubscribers = new BusyMessageHandlerType(1 << 1);
        public static readonly BusyMessageHandlerType HandleAndNotifySubscribers = Handle | NotifySubscribers;

        #endregion

        #region Constructors

        static BusyMessageHandlerType()
        {
            SetIsFlagEnum(i => new BusyMessageHandlerType(i));
        }


        [Preserve(Conditional = true)]
        protected BusyMessageHandlerType()
        {
        }

        public BusyMessageHandlerType(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BusyMessageHandlerType? left, BusyMessageHandlerType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BusyMessageHandlerType? left, BusyMessageHandlerType? right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BusyMessageHandlerType operator |(BusyMessageHandlerType left, BusyMessageHandlerType right)
        {
            Should.NotBeNull(left, nameof(left));
            Should.NotBeNull(right, nameof(right));
            return Parse(left.Value | right.Value);
        }

        [Pure]
        public bool HasFlag(BusyMessageHandlerType flag)
        {
            return (Value & flag.Value) == flag.Value;
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}