using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class UnaryTokenType : EnumBase<UnaryTokenType, string>
    {
        public static readonly UnaryTokenType DynamicExpression = new("$") {IsSingleExpression = true};
        public static readonly UnaryTokenType StaticExpression = new("$$") {IsSingleExpression = true};
        public static readonly UnaryTokenType Minus = new("-");
        public static readonly UnaryTokenType Plus = new("+");
        public static readonly UnaryTokenType LogicalNegation = new("!");
        public static readonly UnaryTokenType BitwiseNegation = new("~");

        public UnaryTokenType(string value, string? name = null, bool register = true)
            : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected UnaryTokenType()
        {
        }

        public bool IsSingleExpression { get; set; }
    }
}