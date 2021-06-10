using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/index
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class BinaryTokenType : EnumBase<BinaryTokenType, string>
    {
        public static readonly BinaryTokenType Multiplication = new("*", 990);
        public static readonly BinaryTokenType Division = new("/", 990);
        public static readonly BinaryTokenType Remainder = new("%", 990, "mod");
        public static readonly BinaryTokenType Addition = new("+", 980);
        public static readonly BinaryTokenType Subtraction = new("-", 980);
        public static readonly BinaryTokenType LeftShift = new("<<", 970);
        public static readonly BinaryTokenType RightShift = new(">>", 970);
        public static readonly BinaryTokenType LessThan = new("<", 960, "&lt;");
        public static readonly BinaryTokenType GreaterThan = new(">", 960, "&gt;");
        public static readonly BinaryTokenType LessThanOrEqual = new("<=", 960);
        public static readonly BinaryTokenType GreaterThanOrEqual = new(">=", 960);
        public static readonly BinaryTokenType Equality = new("==", 950);
        public static readonly BinaryTokenType NotEqual = new("!=", 950);
        public static readonly BinaryTokenType LogicalAnd = new("&", 940, "&amp;");
        public static readonly BinaryTokenType LogicalXor = new("^", 930);
        public static readonly BinaryTokenType LogicalOr = new("|", 920);
        public static readonly BinaryTokenType ConditionalAnd = new("&&", 910, "and");
        public static readonly BinaryTokenType ConditionalOr = new("||", 900, "or");
        public static readonly BinaryTokenType NullCoalescing = new("??", 890);
        public static readonly BinaryTokenType Assignment = new("=", 880);

        public BinaryTokenType(string value, int priority)
            : this(value, null, priority, default)
        {
        }

        public BinaryTokenType(string value, int priority, ItemOrArray<string> aliases)
            : this(value, null, priority, aliases)
        {
        }

        public BinaryTokenType(string value, string? name, int priority, ItemOrArray<string> aliases)
            : base(value, name)
        {
            Priority = priority;
            Aliases = aliases;
        }

        [Preserve(Conditional = true)]
        protected BinaryTokenType()
        {
        }

        [DataMember(Name = "P")]
        public int Priority { get; set; }

        [DataMember(Name = "A")]
        public ItemOrArray<string> Aliases { get; set; }
    }
}