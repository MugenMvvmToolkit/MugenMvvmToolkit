using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Should;

namespace MugenMvvm.UnitTest
{
    public static class UnitTestExtensions
    {
        #region Methods

        public static object? Invoke(this Expression expression, params object?[] args)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke(args);
        }

        public static object? Invoke(this Expression expression, IEnumerable<Expression> parameters, params object?[] args)
        {
            return Expression.Lambda(expression, parameters.OfType<ParameterExpression>()).Compile().DynamicInvoke(args);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, IEnumerable<T> itemsEnumerable)
        {
            foreach (var item in itemsEnumerable)
                CollectionAssertExtensions.ShouldContain(enumerable, item);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, params T[] items)
        {
            ShouldContain(enumerable, itemsEnumerable: items);
        }

        public static void ShouldBeNull(this object @object, string msg)
        {
            @object.ShouldBeNull();
        }

        #endregion
    }
}