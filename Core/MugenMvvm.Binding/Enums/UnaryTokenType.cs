using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
#pragma warning disable 660,661
    public class UnaryTokenType : EnumBase<UnaryTokenType, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly UnaryTokenType DynamicExpression = new UnaryTokenType("$") { IsSingleExpression = true };
        public static readonly UnaryTokenType StaticExpression = new UnaryTokenType("$$") { IsSingleExpression = true };
        public static readonly UnaryTokenType Minus = new UnaryTokenType("-");
        public static readonly UnaryTokenType Plus = new UnaryTokenType("+");
        public static readonly UnaryTokenType LogicalNegation = new UnaryTokenType("!");
        public static readonly UnaryTokenType BitwiseNegation = new UnaryTokenType("~");

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected UnaryTokenType()
        {
        }

        public UnaryTokenType(string value)
            : base(value)
        {
        }

        #endregion

        #region Properties

        public bool IsSingleExpression { get; set; }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnaryTokenType? left, UnaryTokenType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnaryTokenType? left, UnaryTokenType? right)
        {
            return !(left == right);
        }

        protected override bool Equals(string value)
        {
            return Value.Equals(value);
        }

        #endregion
    }
}