using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public class ExpressionNodeType : EnumBase<ExpressionNodeType, int>
    {
        public static readonly ExpressionNodeType Binary = new(1);
        public static readonly ExpressionNodeType Condition = new(2);
        public static readonly ExpressionNodeType Constant = new(3);
        public static readonly ExpressionNodeType Index = new(4);
        public static readonly ExpressionNodeType Member = new(5);
        public static readonly ExpressionNodeType MethodCall = new(6);
        public static readonly ExpressionNodeType Unary = new(7);
        public static readonly ExpressionNodeType Lambda = new(8);
        public static readonly ExpressionNodeType Parameter = new(9);
        public static readonly ExpressionNodeType BindingParameter = new(10);
        public static readonly ExpressionNodeType TypeAccess = new(11);

        public ExpressionNodeType(int value, string? name = null) : base(value, name)
        {
        }

        [Preserve(Conditional = true)]
        protected ExpressionNodeType()
        {
        }
    }
}