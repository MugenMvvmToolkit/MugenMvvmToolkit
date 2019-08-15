using System.Runtime.Serialization;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/index
    public class BinaryTokenType : EnumBase<BinaryTokenType, string>
    {
        #region Fields

        public static readonly BinaryTokenType NullConditionalMemberAccess = new BinaryTokenType("?.", 1000);
        public static readonly BinaryTokenType Multiplication = new BinaryTokenType("*", 990);
        public static readonly BinaryTokenType Division = new BinaryTokenType("/", 990);
        public static readonly BinaryTokenType Remainder = new BinaryTokenType("%", 990, "mod");
        public static readonly BinaryTokenType Addition = new BinaryTokenType("+", 980);
        public static readonly BinaryTokenType Subtraction = new BinaryTokenType("-", 980);
        public static readonly BinaryTokenType LeftShift = new BinaryTokenType("<<", 970);
        public static readonly BinaryTokenType RightShift = new BinaryTokenType(">>", 970);
        public static readonly BinaryTokenType LessThan = new BinaryTokenType("<", 960);
        public static readonly BinaryTokenType GreaterThan = new BinaryTokenType(">", 960);
        public static readonly BinaryTokenType LessThanOrEqualTo = new BinaryTokenType("<=", 960);
        public static readonly BinaryTokenType GreaterThanOrEqualTo = new BinaryTokenType(">=", 960);
        public static readonly BinaryTokenType Equality = new BinaryTokenType("==", 950, "=");
        public static readonly BinaryTokenType NotEqual = new BinaryTokenType("!=", 950);
        public static readonly BinaryTokenType LogicalAnd = new BinaryTokenType("&", 940);
        public static readonly BinaryTokenType LogicalXor = new BinaryTokenType("^", 930);
        public static readonly BinaryTokenType LogicalOr = new BinaryTokenType("|", 920);
        public static readonly BinaryTokenType ConditionalAnd = new BinaryTokenType("&&", 910, "and");
        public static readonly BinaryTokenType ConditionalOr = new BinaryTokenType("||", 900, "or");
        public static readonly BinaryTokenType NullCoalescing = new BinaryTokenType("??", 890);

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

        [DataMember(Name = "P")]
        public int Priority { get; set; }

        [DataMember(Name = "A")]
        public string[]? Aliases { get; set; }

        #endregion
    }
}