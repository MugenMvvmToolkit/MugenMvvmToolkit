using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ExpressionNodeType : EnumBase<ExpressionNodeType, string>
    {
        public static readonly ExpressionNodeType Binary = new(nameof(Binary));
        public static readonly ExpressionNodeType Condition = new(nameof(Condition));
        public static readonly ExpressionNodeType Constant = new(nameof(Constant));
        public static readonly ExpressionNodeType Index = new(nameof(Index));
        public static readonly ExpressionNodeType Member = new(nameof(Member));
        public static readonly ExpressionNodeType MethodCall = new(nameof(MethodCall));
        public static readonly ExpressionNodeType Unary = new(nameof(Unary));
        public static readonly ExpressionNodeType Lambda = new(nameof(Lambda));
        public static readonly ExpressionNodeType Parameter = new(nameof(Parameter));
        public static readonly ExpressionNodeType BindingParameter = new(nameof(BindingParameter));
        public static readonly ExpressionNodeType TypeAccess = new(nameof(TypeAccess));

        public ExpressionNodeType(string value, string? name = null, bool register = true) : base(value, name, register)
        {
        }

        [Preserve(Conditional = true)]
        protected ExpressionNodeType()
        {
        }
    }
}