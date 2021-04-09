using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class ConstantExpressionNode : ExpressionNodeBase<IConstantExpressionNode>, IConstantExpressionNode
    {
        public static readonly ConstantExpressionNode True = new(BoxingExtensions.TrueObject, typeof(bool), MugenExtensions.TrueConstantExpression);
        public static readonly ConstantExpressionNode False = new(BoxingExtensions.FalseObject, typeof(bool), MugenExtensions.FalseConstantExpression);
        public static readonly ConstantExpressionNode Null = new(null, typeof(object), MugenExtensions.NullConstantExpression);
        public static readonly ConstantExpressionNode EmptyString = new("", typeof(string), Expression.Constant(""));

        public ConstantExpressionNode(object? value, Type? type = null, ConstantExpression? constantExpression = null,
            IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();
            else if (value != null)
                Should.BeOfType(value, type, nameof(value));
            Value = value;
            Type = type;
            ConstantExpression = constantExpression;
        }

        public ConstantExpression? ConstantExpression { get; }

        public Type Type { get; }

        public object? Value { get; }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Constant;

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

        public override string ToString()
        {
            if (Value == null)
                return InternalConstant.Null;
            if (Value is bool)
                return Value.ToString()!.ToLowerInvariant();
            if (Value is string v)
                return $"\"{v}\"";
            if (Value is char)
                return $"'{Value}'";
            return Value.ToString()!;
        }

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        protected override IConstantExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new ConstantExpressionNode(Value, Type, ConstantExpression, metadata);

        protected override bool Equals(IConstantExpressionNode other, IExpressionEqualityComparer? comparer) => Type == other.Type && Equals(Value, other.Value);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => HashCode.Combine(hashCode, Type, Value);

        private static class IntCache
        {
            public static readonly ConstantExpressionNode[] Positive = GenerateItems(MugenExtensions.IntCache.Positive);
            public static readonly ConstantExpressionNode[] Negative = GenerateItems(MugenExtensions.IntCache.Negative);

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
        }
    }
}