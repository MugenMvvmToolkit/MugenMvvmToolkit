using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Fields

        public static readonly ConstantExpression NullConstantExpression = Expression.Constant(null, typeof(object));
        public static readonly ConstantExpression TrueConstantExpression = Expression.Constant(BoxingExtensions.TrueObject);
        public static readonly ConstantExpression FalseConstantExpression = Expression.Constant(BoxingExtensions.FalseObject);

        #endregion

        #region Methods

        public static ConstantExpression GetConstantExpression(byte value)
        {
            return ExpressionCache<byte>.Items[value];
        }

        public static ConstantExpression GetConstantExpression(sbyte value)
        {
            if (value < 0)
                return ExpressionCache<sbyte>.NegativeItems[-value];
            return ExpressionCache<sbyte>.Items[value];
        }

        public static ConstantExpression GetConstantExpression(ushort value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<ushort>.Items[value];
            return Expression.Constant(value);
        }

        public static ConstantExpression GetConstantExpression(short value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<short>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<short>.Items[value];

            return Expression.Constant(value);
        }

        public static ConstantExpression GetConstantExpression(uint value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<uint>.Items[value];
            return Expression.Constant(value);
        }

        public static ConstantExpression GetConstantExpression(int value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<int>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<int>.Items[value];

            return Expression.Constant(value);
        }

        public static ConstantExpression GetConstantExpression(ulong value)
        {
            if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<ulong>.Items[value];
            return Expression.Constant(value);
        }

        public static ConstantExpression GetConstantExpression(long value)
        {
            if (value < 0)
            {
                if (value > -BoxingExtensions.CacheSize)
                    return ExpressionCache<long>.NegativeItems[-value];
            }
            else if (value < BoxingExtensions.CacheSize)
                return ExpressionCache<long>.Items[value];

            return Expression.Constant(value);
        }

        #endregion

        #region Nested types

        internal static class ExpressionCache<T>
        {
            #region Fields

            public static readonly ConstantExpression[] Items = GenerateItems(false);
            public static readonly ConstantExpression[] NegativeItems = GenerateItems(true);

            #endregion

            #region Methods

            private static ConstantExpression[] GenerateItems(bool negative)
            {
                var cache = negative ? BoxingExtensions.Cache<T>.NegativeItems : BoxingExtensions.Cache<T>.Items;
                if (cache.Length == 0)
                    return Default.EmptyArray<ConstantExpression>();

                var items = new ConstantExpression[cache.Length];
                for (var i = 0; i < items.Length; i++)
                    items[i] = Expression.Constant(cache[i]);
                return items;
            }

            #endregion
        }

        #endregion
    }
}