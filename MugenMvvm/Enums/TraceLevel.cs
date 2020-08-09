using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
#pragma warning disable 660,661
    public class TraceLevel : EnumBase<TraceLevel, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly TraceLevel Information = new TraceLevel(nameof(Information));
        public static readonly TraceLevel Warning = new TraceLevel(nameof(Warning));
        public static readonly TraceLevel Error = new TraceLevel(nameof(Error));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected TraceLevel()
        {
        }

        public TraceLevel(string value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TraceLevel? left, TraceLevel? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TraceLevel? left, TraceLevel? right) => !(left == right);

        protected override bool Equals(string value) => Value.Equals(value);

        #endregion
    }
}