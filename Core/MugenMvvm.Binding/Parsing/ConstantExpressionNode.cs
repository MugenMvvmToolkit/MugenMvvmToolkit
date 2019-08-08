using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class ConstantExpressionNode : ExpressionNodeBase, IConstantExpressionNode
    {
        #region Constructors

        public ConstantExpressionNode(object? value, Type? type = null)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();
            Value = value;
            Type = type;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Constant;

        public Type Type { get; }

        public object Value { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value.ToString();
        }

        #endregion
    }
}