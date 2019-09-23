using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class ConstantExpressionNode : ExpressionNodeBase, IConstantExpressionNode
    {
        #region Fields

        public static readonly ConstantExpressionNode True = new ConstantExpressionNode(Default.TrueObject, typeof(bool));
        public static readonly ConstantExpressionNode False = new ConstantExpressionNode(Default.FalseObject, typeof(bool));

        #endregion

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

        public object? Value { get; }

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
            if (Value is string v)
                return $"\"{Value}\"";
            if (Value is char)
                return $"'{Value}'";
            return Value.ToString();
        }

        #endregion
    }
}