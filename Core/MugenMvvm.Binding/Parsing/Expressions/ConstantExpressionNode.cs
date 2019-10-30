using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
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
                Should.BeOfType(value, nameof(value), type);
            Value = value;
            Type = type;
            ConstantExpression = constantExpression;
        }

        #endregion

        #region Properties

        public ConstantExpression? ConstantExpression { get; }

        public override ExpressionNodeType NodeType => ExpressionNodeType.Constant;

        public Type Type { get; }

        public object? Value { get; }

        #endregion

        #region Methods

        public static ConstantExpressionNode Get<TType>()
        {
            return TypeCache<TType>.TypeConstant;
        }

        public static ConstantExpressionNode Get(bool value)
        {
            if (value)
                return True;
            return False;
        }

        public static ConstantExpressionNode Get(byte value)
        {
            return ExpressionCache<byte>.Items[value];
        }

        public static ConstantExpressionNode Get(sbyte value)
        {
            if (value < 0)
                return ExpressionCache<sbyte>.NegativeItems[-value];
            return ExpressionCache<sbyte>.Items[value];
        }

        public static ConstantExpressionNode Get(ushort value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<ushort>.Items[value];
            return new ConstantExpressionNode(value, typeof(ushort));
        }

        public static ConstantExpressionNode Get(short value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<short>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<short>.Items[value];

            return new ConstantExpressionNode(value, typeof(short));
        }

        public static ConstantExpressionNode Get(uint value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<uint>.Items[value];
            return new ConstantExpressionNode(value, typeof(uint));
        }

        public static ConstantExpressionNode Get(int value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<int>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<int>.Items[value];

            return new ConstantExpressionNode(value, typeof(int));
        }

        public static ConstantExpressionNode Get(ulong value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<ulong>.Items[value];
            return new ConstantExpressionNode(value, typeof(ulong));
        }

        public static ConstantExpressionNode Get(long value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<long>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<long>.Items[value];

            return new ConstantExpressionNode(value, typeof(long));
        }

        public static ConstantExpressionNode Get(object? value, Type? type = null)
        {
            if (value == null && (type == null || typeof(object) == type))
                return Null;
            if (value is bool b)
                return Get(b);
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
            if (Value is bool)
                return Value.ToString().ToLowerInvariant();
            if (Value is string v)
                return $"\"{Value}\"";
            if (Value is char)
                return $"'{Value}'";
            return Value.ToString();
        }

        #endregion

        #region Nested types

        private static class ExpressionCache<T>
        {
            #region Fields

            public static readonly ConstantExpressionNode[] Items = GenerateItems(false);
            public static readonly ConstantExpressionNode[] NegativeItems = GenerateItems(true);

            #endregion

            #region Methods

            private static ConstantExpressionNode[] GenerateItems(bool negative)
            {
                var cache = negative ? MugenExtensions.ExpressionCache<T>.NegativeItems : MugenExtensions.ExpressionCache<T>.Items;
                if (cache.Length == 0)
                    return Default.EmptyArray<ConstantExpressionNode>();

                var items = new ConstantExpressionNode[cache.Length];
                for (var i = 0; i < items.Length; i++)
                {
                    var constantExpression = cache[i];
                    items[i] = new ConstantExpressionNode(constantExpression.Value, constantExpression.Type, constantExpression);
                }

                return items;
            }

            #endregion
        }

        private static class TypeCache<TType>
        {
            #region Fields

            public static readonly ConstantExpressionNode TypeConstant = new ConstantExpressionNode(typeof(TType), typeof(Type), Expression.Constant(typeof(TType)));

            #endregion
        }

        #endregion
    }
}