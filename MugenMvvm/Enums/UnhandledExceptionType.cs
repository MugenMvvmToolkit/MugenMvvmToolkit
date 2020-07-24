using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
#pragma warning disable 660,661
    public class UnhandledExceptionType : EnumBase<UnhandledExceptionType, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly UnhandledExceptionType Binding = new UnhandledExceptionType(1);
        public static readonly UnhandledExceptionType System = new UnhandledExceptionType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected UnhandledExceptionType()
        {
        }

        public UnhandledExceptionType(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnhandledExceptionType? left, UnhandledExceptionType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnhandledExceptionType? left, UnhandledExceptionType? right)
        {
            return !(left == right);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}