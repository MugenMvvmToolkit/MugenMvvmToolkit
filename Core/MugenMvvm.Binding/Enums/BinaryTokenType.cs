using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BinaryTokenType : EnumBase<BinaryTokenType, string>
    {
        #region Fields

        public static readonly BinaryTokenType NullConditionalMemberAccess = new BinaryTokenType("?.", 1000);
        public static readonly BinaryTokenType Multiplication = new BinaryTokenType("*", 990);
        public static readonly BinaryTokenType Division = new BinaryTokenType("/", 980);
        public static readonly BinaryTokenType Remainder = new BinaryTokenType("%", 970, "mod");
        public static readonly BinaryTokenType Addition = new BinaryTokenType("+", 960);
        public static readonly BinaryTokenType Subtraction = new BinaryTokenType("-", 950);
        public static readonly BinaryTokenType LessThan = new BinaryTokenType("<", 940);
        public static readonly BinaryTokenType GreaterThan = new BinaryTokenType(">", 930);
        public static readonly BinaryTokenType LessThanOrEqualTo = new BinaryTokenType("<=", 920);
        public static readonly BinaryTokenType GreaterThanOrEqualTo = new BinaryTokenType(">=", 900);
        public static readonly BinaryTokenType Equality = new BinaryTokenType("==", 890, "=");
        public static readonly BinaryTokenType NotEqual = new BinaryTokenType("!=", 880);
        public static readonly BinaryTokenType LogicalAnd = new BinaryTokenType("&", 870);
        public static readonly BinaryTokenType LogicalXor = new BinaryTokenType("^", 860);
        public static readonly BinaryTokenType LogicalOr = new BinaryTokenType("|", 850);
        public static readonly BinaryTokenType ConditionalAnd = new BinaryTokenType("&&", 840, "and");
        public static readonly BinaryTokenType ConditionalOr = new BinaryTokenType("||", 830, "or");
        public static readonly BinaryTokenType NullCoalescing = new BinaryTokenType("??", 820);

        #endregion

        #region Constructors

        protected BinaryTokenType()
        {
        }

        public BinaryTokenType(string value, int priority)
            : this(value, priority, null)
        {
            Priority = priority;
        }

        public BinaryTokenType(string value, int priority, params string[]? aliases)
            : base(value)
        {
            Priority = priority;
            Aliases = aliases;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public string[] Aliases { get; set; }

        #endregion
    }
}