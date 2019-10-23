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
        public static readonly ConstantExpressionNode Null = new ConstantExpressionNode(null, typeof(object));
        public static readonly ConstantExpressionNode EmptyString = new ConstantExpressionNode("", typeof(string));

        #endregion

        #region Constructors

        public ConstantExpressionNode(object? value, Type? type = null)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();
            else if (value != null)
                Should.BeOfType(value, nameof(value), type);
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

        public static ConstantExpressionNode Get<TType>()
        {
            return StaticTypeHolder<TType>.TypeConstant;
        }

        public static ConstantExpressionNode Get(object? value, Type? type = null)
        {
            if (value == null && (type == null || typeof(object).EqualsEx(type)))
                return Null;
            if (value is bool b)
            {
                if (b)
                    return True;
                return False;
            }

            return new ConstantExpressionNode(value, type);
        }

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

        #region Nested types

        private static class StaticTypeHolder<TType>
        {
            #region Fields

            public static readonly ConstantExpressionNode TypeConstant = new ConstantExpressionNode(typeof(TType), typeof(Type));

            #endregion
        }

        #endregion
    }
}