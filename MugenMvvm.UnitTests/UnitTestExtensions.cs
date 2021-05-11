using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using Should;
using Should.Core.Assertions;

namespace MugenMvvm.UnitTests
{
    public static class UnitTestExtensions
    {
        public static void ShouldEqual<T>(this ItemOrArray<T> itemOrList, IEnumerable<T> enumerable) => itemOrList.AsList().ShouldEqual(enumerable);

        public static void ShouldContain<T>(this ItemOrArray<T> itemOrList, params T[] values) => itemOrList.AsList().ShouldContain(values);

        public static void ShouldBeEmpty<T>(this ItemOrArray<T> itemOrList) => itemOrList.IsEmpty.ShouldBeTrue();

        public static T Single<T>(this ItemOrArray<T> itemOrList)
        {
            if (itemOrList.Count != 1)
                throw new InvalidOperationException("Assert Single");
            return itemOrList[0];
        }

        public static T UpdateMetadata<T>(this T expression, string key1, object? value1, string? key2 = null, object? value2 = null)
            where T : class, IExpressionNode
        {
            var dictionary = new Dictionary<string, object?> {[key1] = value1};
            if (key2 != null)
                dictionary[key2] = value2;
            return (T) expression.UpdateMetadata(dictionary);
        }

        public static async Task WaitSafeAsync<T>(this ValueTask<T> task)
        {
            try
            {
                await task;
            }
            catch
            {
                ;
            }
        }

        public static async Task WaitSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                ;
            }
        }

        public static object? Invoke(this Expression expression, params object?[] args) => Expression.Lambda(expression).Compile().DynamicInvoke(args);

        public static object? Invoke(this Expression expression, IEnumerable<Expression> parameters, params object?[] args) =>
            Expression.Lambda(expression, parameters.OfType<ParameterExpression>()).Compile().DynamicInvoke(args);

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, IEnumerable<T> itemsEnumerable)
        {
            foreach (var item in itemsEnumerable)
                CollectionAssertExtensions.ShouldContain(enumerable, item);
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, IEnumerable<T> itemsEnumerable)
        {
            foreach (var item in itemsEnumerable)
                enumerable.ShouldNotContain(item);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, params T[] items) => ShouldContain(enumerable, itemsEnumerable: items);

        public static void ShouldBeNull(this object @object, string msg) => @object.ShouldBeNull();

        public static void ShouldEqual(this IExpressionNode? x1, IExpressionNode? x2) => x1!.ShouldEqual(x2!, ExpressionNodeEqualityComparer.Instance);

        public static void ShouldEqual(this IEnumerable<IExpressionNode> first, IEnumerable<IExpressionNode> second) =>
            first.SequenceEqual(second, ExpressionNodeEqualityComparer.Instance).ShouldBeTrue();

        public static void ShouldEqual(this IReadOnlyMetadataContext? x1, IReadOnlyMetadataContext? x2)
        {
            if (!ReferenceEquals(x1, x2))
            {
                if (x1 != null && x2 != null)
                    x1.GetValues().AsList().ShouldEqual(x2.GetValues().AsList());
                else
                    ObjectAssertExtensions.ShouldEqual(x1, x2);
            }
        }

        public static void ShouldEqual<T>(this IEnumerable<T>? enumerable, IEnumerable<T>? value) => Assert.Equal(enumerable, value);

        public static void ShouldEqual<T>(this IEnumerable<T>? enumerable, IEnumerable<T>? value, IEqualityComparer<T> comparer)
        {
            if (EqualityComparer<IEnumerable<T>?>.Default.Equals(enumerable, value))
                return;
            enumerable!.ShouldNotBeNull(nameof(enumerable));
            value!.ShouldNotBeNull(nameof(value));
            enumerable!.SequenceEqual(value!, comparer).ShouldBeTrue();
        }

        public static bool EqualsEx(this IExpressionNode? x1, IExpressionNode? x2)
        {
            if (ReferenceEquals(x1, x2))
                return true;
            if (ReferenceEquals(x1, null) || ReferenceEquals(x2, null))
                return false;
            if (x1.GetHashCode() != x2.GetHashCode())
                return false;
            return x1.Equals(x2);
        }

        private sealed class ExpressionNodeEqualityComparer : IEqualityComparer<IExpressionNode>
        {
            public static readonly ExpressionNodeEqualityComparer Instance = new();

            private ExpressionNodeEqualityComparer()
            {
            }

            public bool Equals(IExpressionNode? x, IExpressionNode? y) => x.EqualsEx(y);

            public int GetHashCode(IExpressionNode? obj) => 0;
        }
    }
}