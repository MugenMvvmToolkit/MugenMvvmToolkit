using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class UnaryExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        public UnaryExpressionBuilder()
        {
            Mapping = new Dictionary<UnaryTokenType, Func<Expression, Expression>>
            {
                [UnaryTokenType.Minus] = Expression.Negate,
                [UnaryTokenType.Plus] = Expression.UnaryPlus,
                [UnaryTokenType.LogicalNegation] = Expression.Not,
                [UnaryTokenType.BitwiseNegation] = Expression.Not
            };
        }

        public Dictionary<UnaryTokenType, Func<Expression, Expression>> Mapping { get; }

        public int Priority { get; set; } = CompilingComponentPriority.Unary;

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is IUnaryExpressionNode unaryExpressionNode)
            {
                if (Mapping.TryGetValue(unaryExpressionNode.Token, out var func))
                    return func(context.Build(unaryExpressionNode.Operand));

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileUnaryExpressionFormat2.Format(expression, unaryExpressionNode.Token));
            }

            return null;
        }
    }
}