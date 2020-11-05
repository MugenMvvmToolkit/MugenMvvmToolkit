using System;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class ExpressionNodeType : EnumBase<ExpressionNodeType, int>
    {
        #region Fields

        public static readonly ExpressionNodeType Binary = new ExpressionNodeType(1);
        public static readonly ExpressionNodeType Condition = new ExpressionNodeType(2);
        public static readonly ExpressionNodeType Constant = new ExpressionNodeType(3);
        public static readonly ExpressionNodeType Index = new ExpressionNodeType(4);
        public static readonly ExpressionNodeType Member = new ExpressionNodeType(5);
        public static readonly ExpressionNodeType MethodCall = new ExpressionNodeType(6);
        public static readonly ExpressionNodeType Unary = new ExpressionNodeType(7);
        public static readonly ExpressionNodeType Lambda = new ExpressionNodeType(8);
        public static readonly ExpressionNodeType Parameter = new ExpressionNodeType(9);
        public static readonly ExpressionNodeType BindingMember = new ExpressionNodeType(10);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ExpressionNodeType()
        {
        }

        public ExpressionNodeType(int value) : base(value)
        {
        }

        public ExpressionNodeType(int value, string name) : base(value, name)
        {
        }

        #endregion
    }
}