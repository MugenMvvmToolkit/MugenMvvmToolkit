using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
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

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, IEnumerable<T> itemsEnumerable)
        {
            foreach (var item in itemsEnumerable)
                enumerable.ShouldNotContain(item);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, params T[] items)
        {
            ShouldContain(enumerable, itemsEnumerable: items);
        }

        public static void ShouldBeNull(this object @object, string msg)
        {
            @object.ShouldBeNull();
        }

        public static void ShouldEqual(this IExpressionNode? x1, IExpressionNode? x2)
        {
            x1.EqualsEx(x2).ShouldBeTrue();
        }

        public static bool EqualsEx(this IExpressionNode? x1, IExpressionNode? x2)
        {
            if (x1 == null || x2 == null)
                return x1 == x2;
            if (x1.GetType() != x2.GetType())
                return false;
            switch (x1)
            {
                case IBinaryExpressionNode binaryExpressionNode:
                    return binaryExpressionNode.EqualsEx((IBinaryExpressionNode)x2);
                case IBindingMemberExpressionNode bindingMemberExpressionNode:
                    return bindingMemberExpressionNode.EqualsEx((IBindingMemberExpressionNode)x2);
                case IConditionExpressionNode conditionExpressionNode:
                    return conditionExpressionNode.EqualsEx((IConditionExpressionNode)x2);
                case IConstantExpressionNode constantExpressionNode:
                    return constantExpressionNode.EqualsEx((IConstantExpressionNode)x2);
                case IIndexExpressionNode indexExpressionNode:
                    return indexExpressionNode.EqualsEx((IIndexExpressionNode)x2);
                case ILambdaExpressionNode lambdaExpressionNode:
                    return lambdaExpressionNode.EqualsEx((ILambdaExpressionNode)x2);
                case IMemberExpressionNode memberExpressionNode:
                    return memberExpressionNode.EqualsEx((IMemberExpressionNode)x2);
                case IMethodCallExpressionNode methodCallExpressionNode:
                    return methodCallExpressionNode.EqualsEx((IMethodCallExpressionNode)x2);
                case IParameterExpressionNode parameterExpressionNode:
                    return parameterExpressionNode.EqualsEx((IParameterExpressionNode)x2);
                case IUnaryExpressionNode unaryExpressionNode:
                    return unaryExpressionNode.EqualsEx((IUnaryExpressionNode)x2);
                case NullConditionalMemberExpressionNode nullConditionalMember:
                    return nullConditionalMember.EqualsEx((NullConditionalMemberExpressionNode)x2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(x1));
            }
        }

        private static bool EqualsEx(this NullConditionalMemberExpressionNode x1, NullConditionalMemberExpressionNode x2)
        {
            return x1.Target.EqualsEx(x2.Target);
        }

        private static bool EqualsEx(this IBinaryExpressionNode x1, IBinaryExpressionNode x2)
        {
            return x1.Token == x2.Token && x1.Left.EqualsEx(x2.Left) && x1.Right.EqualsEx(x2.Right);
        }

        private static bool EqualsEx(this IBindingMemberExpressionNode x1, IBindingMemberExpressionNode x2)
        {
            if (x1.Path != x2.Path || x1.Index != x2.Index || x1.Flags != x2.Flags)
                return false;
            switch (x1)
            {
                case BindingMemberExpressionNode bindingMemberExpressionNode:
                    return bindingMemberExpressionNode.Type == ((BindingMemberExpressionNode)x2).Type;
                case BindingInstanceMemberExpressionNode bindingInstanceMemberExpressionNode:
                    return Equals(bindingInstanceMemberExpressionNode.Instance, ((BindingInstanceMemberExpressionNode)x2).Instance);
                case BindingResourceMemberExpressionNode bindingResourceMemberExpressionNode:
                    return bindingResourceMemberExpressionNode.ResourceName == ((BindingResourceMemberExpressionNode)x2).ResourceName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(x1));
            }
        }

        private static bool EqualsEx(this IConditionExpressionNode x1, IConditionExpressionNode x2)
        {
            return x1.Condition.EqualsEx(x2.Condition) && x1.IfFalse.EqualsEx(x2.IfFalse) && x1.IfTrue.EqualsEx(x2.IfTrue);
        }

        private static bool EqualsEx(this IConstantExpressionNode x1, IConstantExpressionNode x2)
        {
            return x1.Type == x2.Type && Equals(x1.Value, x2.Value);
        }

        private static bool EqualsEx(this IIndexExpressionNode x1, IIndexExpressionNode x2)
        {
            return x1.Target.EqualsEx(x2.Target) && x1.Arguments.EqualsEx(x2.Arguments);
        }

        private static bool EqualsEx(this ILambdaExpressionNode x1, ILambdaExpressionNode x2)
        {
            return x1.Body.EqualsEx(x2.Body) && x1.Parameters.EqualsEx(x2.Parameters);
        }

        private static bool EqualsEx(this IMemberExpressionNode x1, IMemberExpressionNode x2)
        {
            return x1.Member == x2.Member && x1.Target.EqualsEx(x2.Target);
        }

        private static bool EqualsEx(this IMethodCallExpressionNode x1, IMethodCallExpressionNode x2)
        {
            return x1.Method == x2.Method && x1.TypeArgs.SequenceEqual(x2.TypeArgs) && x1.Target.EqualsEx(x2.Target) && x1.Arguments.EqualsEx(x2.Arguments);
        }

        private static bool EqualsEx(this IParameterExpressionNode x1, IParameterExpressionNode x2)
        {
            return x1.Name == x2.Name;
        }

        private static bool EqualsEx(this IUnaryExpressionNode x1, IUnaryExpressionNode x2)
        {
            return x1.Token == x2.Token && x1.Operand.EqualsEx(x2.Operand);
        }

        private static bool EqualsEx<T>(this IReadOnlyList<T> x1, IReadOnlyList<T> x2) where T : class, IExpressionNode
        {
            if (x1.Count != x2.Count)
                return false;
            for (var i = 0; i < x1.Count; i++)
            {
                if (!x1[i].EqualsEx(x2[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}