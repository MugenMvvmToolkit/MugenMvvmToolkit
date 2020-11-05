using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class UnaryTokenType : EnumBase<UnaryTokenType, string>
    {
        #region Fields

        public static readonly UnaryTokenType DynamicExpression = new UnaryTokenType("$") {IsSingleExpression = true};
        public static readonly UnaryTokenType StaticExpression = new UnaryTokenType("$$") {IsSingleExpression = true};
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

        public bool IsSingleExpression { get; protected internal set; }

        #endregion
    }
}