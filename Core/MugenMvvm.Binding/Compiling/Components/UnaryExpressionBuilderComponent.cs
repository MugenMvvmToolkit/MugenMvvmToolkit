using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class UnaryExpressionBuilderComponent : IExpressionBuilderComponent, IHasPriority
    {
        #region Constructors

        public UnaryExpressionBuilderComponent()
        {
            UnaryTokenMapping = new Dictionary<UnaryTokenType, Func<Expression, Expression>>
            {
                [UnaryTokenType.Minus] = Expression.Negate,
                [UnaryTokenType.Plus] = Expression.UnaryPlus,
                [UnaryTokenType.LogicalNegation] = Expression.Not,
                [UnaryTokenType.BitwiseNegation] = Expression.Not
            };
        }

        #endregion

        #region Properties

        public Dictionary<UnaryTokenType, Func<Expression, Expression>> UnaryTokenMapping { get; }

        public int Priority { get; set; } = CompilingComponentPriority.Unary;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is IUnaryExpressionNode unaryExpressionNode)
            {
                if (UnaryTokenMapping.TryGetValue(unaryExpressionNode.Token, out var func))
                    return func(context.Build(unaryExpressionNode.Operand));

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileUnaryExpressionFormat2.Format(expression, unaryExpressionNode.Token));
            }
            return null;
        }

        #endregion
    }
}