using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
#pragma warning disable 660,661
    public class MessengerResult : EnumBase<MessengerResult, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly MessengerResult Handled = new MessengerResult(1);
        public static readonly MessengerResult Ignored = new MessengerResult(2);
        public static readonly MessengerResult Invalid = new MessengerResult(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected MessengerResult()
        {
        }

        public MessengerResult(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MessengerResult? left, MessengerResult? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MessengerResult? left, MessengerResult? right) => !(left == right);

        protected override bool Equals(int value) => Value == value;

        #endregion
    }
}