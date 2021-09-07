using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class BinaryExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        private static readonly MethodInfo StringConcatMethod =
            typeof(string).GetMethodOrThrow(nameof(string.Concat), BindingFlagsEx.StaticPublic, new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo EqualsMethod = typeof(object).GetMethodOrThrow(nameof(Equals), BindingFlagsEx.StaticPublic);

        public BinaryExpressionBuilder()
        {
            Mapping = new Dictionary<BinaryTokenType, Func<Expression, Expression, Expression>>
            {
                [BinaryTokenType.Multiplication] = (l, r) => l.GenerateExpression(r, Expression.Multiply),
                [BinaryTokenType.Division] = (l, r) => l.GenerateExpression(r, Expression.Divide),
                [BinaryTokenType.Remainder] = (l, r) => l.GenerateExpression(r, Expression.Modulo),
                [BinaryTokenType.Addition] = GeneratePlusExpression,
                [BinaryTokenType.Subtraction] = (l, r) => l.GenerateExpression(r, Expression.Subtract),
                [BinaryTokenType.LeftShift] = (l, r) => l.GenerateExpression(r, Expression.LeftShift),
                [BinaryTokenType.RightShift] = (l, r) => l.GenerateExpression(r, Expression.RightShift),
                [BinaryTokenType.LessThan] = (l, r) => l.GenerateExpression(r, Expression.LessThan),
                [BinaryTokenType.GreaterThan] = (l, r) => l.GenerateExpression(r, Expression.GreaterThan),
                [BinaryTokenType.LessThanOrEqual] = (l, r) => l.GenerateExpression(r, Expression.LessThanOrEqual),
                [BinaryTokenType.GreaterThanOrEqual] = (l, r) => l.GenerateExpression(r, Expression.GreaterThanOrEqual),
                [BinaryTokenType.Equality] = GenerateEqual,
                [BinaryTokenType.NotEqual] = (l, r) => l.GenerateExpression(r, Expression.NotEqual),
                [BinaryTokenType.LogicalAnd] = (l, r) => l.GenerateExpression(r, Expression.And),
                [BinaryTokenType.LogicalXor] = (l, r) => l.GenerateExpression(r, Expression.ExclusiveOr),
                [BinaryTokenType.LogicalOr] = (l, r) => l.GenerateExpression(r, Expression.Or),
                [BinaryTokenType.ConditionalAnd] = (l, r) => l.GenerateExpression(r, Expression.AndAlso),
                [BinaryTokenType.ConditionalOr] = (l, r) => l.GenerateExpression(r, Expression.OrElse),
                [BinaryTokenType.NullCoalescing] = (l, r) => l.GenerateExpression(r, Expression.Coalesce)
            };
        }

        public Dictionary<BinaryTokenType, Func<Expression, Expression, Expression>> Mapping { get; }

        public int Priority { get; init; } = CompilingComponentPriority.Binary;

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is IBinaryExpressionNode binaryExpression)
            {
                if (Mapping.TryGetValue(binaryExpression.Token, out var func))
                    return func(context.Build(binaryExpression.Left), context.Build(binaryExpression.Right));

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileBinaryExpressionFormat2.Format(expression, binaryExpression.Token));
            }

            return null;
        }

        private static Expression GeneratePlusExpression(Expression left, Expression right)
        {
            if (left.Type == typeof(string) || right.Type == typeof(string))
            {
                return Expression.Call(null, StringConcatMethod, left.ConvertIfNeed(typeof(object), false),
                    right.ConvertIfNeed(typeof(object), false));
            }

            return left.GenerateExpression(right, (x1, x2) => Expression.Add(x1, x2));
        }

        private static Expression GenerateEqual(Expression left, Expression right)
        {
            try
            {
                return left.GenerateExpression(right, (x1, x2) => Expression.Equal(x1, x2));
            }
            catch
            {
                return Expression.Call(null, EqualsMethod, left.ConvertIfNeed(typeof(object), false),
                    right.ConvertIfNeed(typeof(object), false));
            }
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable ReturnValueOfPureMethodIsNotUsed
        [Preserve(Conditional = true)]
        private static void LinkerInclude() => string.Concat(1, 1);
        // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        // ReSharper restore UnusedMember.Local
    }
}