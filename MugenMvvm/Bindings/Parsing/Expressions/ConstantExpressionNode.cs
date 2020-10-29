using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class ConstantExpressionNode : ExpressionNodeBase, IConstantExpressionNode
    {
        #region Fields

        public static readonly ConstantExpressionNode True = new ConstantExpressionNode(BoxingExtensions.TrueObject, typeof(bool), MugenExtensions.TrueConstantExpression);
        public static readonly ConstantExpressionNode False = new ConstantExpressionNode(BoxingExtensions.FalseObject, typeof(bool), MugenExtensions.FalseConstantExpression);
        public static readonly ConstantExpressionNode Null = new ConstantExpressionNode(null, typeof(object), MugenExtensions.NullConstantExpression);
        public static readonly ConstantExpressionNode EmptyString = new ConstantExpressionNode("", typeof(string), Expression.Constant(""));

        #endregion

        #region Constructors

        public ConstantExpressionNode(object? value, Type? type = null, ConstantExpression? constantExpression = null)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();
            else if (value != null)
                Should.BeOfType(value, type, nameof(value));
            Value = value;
            Type = type;
            ConstantExpression = constantExpression;
        }

        #endregion

        #region Properties

        public ConstantExpression? ConstantExpression { get; }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Constant;

        public Type Type { get; }

        public object? Value { get; }

        #endregion

        #region Methods

        public static ConstantExpressionNode Get<TType>() => TypeCache<TType>.TypeConstant;

        public static ConstantExpressionNode Get(bool value)
        {
            if (value)
                return True;
            return False;
        }

        public static ConstantExpressionNode Get(int value)
        {
            if (value < 0)
            {
                if (value >= -BoxingExtensions.CacheSize)
                    return IntCache.Negative[~value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return IntCache.Positive[value];

            return new ConstantExpressionNode(value, typeof(int));
        }

        public static ConstantExpressionNode Get(object? value, Type? type = null)
        {
            if (value == null && (type == null || typeof(object) == type))
                return Null;
            if (value is bool b)
                return Get(b);
            return new ConstantExpressionNode(value, type);
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        public override string ToString()
        {
            if (Value == null)
                return "null";
            if (Value is bool)
                return Value.ToString()!.ToLowerInvariant();
            if (Value is string v)
                return $"\"{v}\"";
            if (Value is char)
                return $"'{Value}'";
            return Value.ToString()!;
        }

        #endregion

        #region Nested types

        private static class IntCache
        {
            #region Fields

            public static readonly ConstantExpressionNode[] Positive = GenerateItems(MugenExtensions.IntCache.Positive);
            public static readonly ConstantExpressionNode[] Negative = GenerateItems(MugenExtensions.IntCache.Negative);

            #endregion

            #region Methods

            private static ConstantExpressionNode[] GenerateItems(ConstantExpression[] values)
            {
                var items = new ConstantExpressionNode[values.Length];
                for (var i = 0; i < items.Length; i++)
                {
                    var constantExpression = values[i];
                    items[i] = new ConstantExpressionNode(constantExpression.Value, constantExpression.Type, constantExpression);
                }

                return items;
            }

            #endregion
        }

        private static class TypeCache<TType>
        {
            #region Fields

            public static readonly ConstantExpressionNode TypeConstant = new ConstantExpressionNode(typeof(TType), typeof(TType).GetType(), Expression.Constant(typeof(TType)));

            #endregion
        }

        #endregion
    }
}