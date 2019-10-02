using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class UnaryTokenType : EnumBase<BinaryTokenType, string>
    {
        #region Fields

        public static readonly UnaryTokenType DynamicExpression = new UnaryTokenType("$");
        public static readonly UnaryTokenType StaticExpression = new UnaryTokenType("$$");
        public static readonly UnaryTokenType Minus = new UnaryTokenType("-");
        public static readonly UnaryTokenType Plus = new UnaryTokenType("+");
        public static readonly UnaryTokenType LogicalNegation = new UnaryTokenType("!");
        public static readonly UnaryTokenType BitwiseNegation = new UnaryTokenType("~");

        #endregion

        #region Constructors

        protected UnaryTokenType()
        {
        }

        public UnaryTokenType(string value)
            : base(value)
        {
        }

        #endregion
    }
}