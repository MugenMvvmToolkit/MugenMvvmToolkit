using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class UnaryExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Constructors

        public UnaryExpressionCompilerComponent(Dictionary<UnaryTokenType, Func<Expression, Expression>>? unaryTokenMapping = null)
        {
            if (unaryTokenMapping == null)
            {
                UnaryTokenMapping = new Dictionary<UnaryTokenType, Func<Expression, Expression>>
                {
                    [UnaryTokenType.Minus] = Expression.Negate,
                    [UnaryTokenType.Plus] = Expression.UnaryPlus,
                    [UnaryTokenType.LogicalNegation] = Expression.Not,
                    [UnaryTokenType.BitwiseNegation] = Expression.Not
                };
            }
            else
                UnaryTokenMapping = unaryTokenMapping;
        }

        #endregion

        #region Properties

        public Dictionary<UnaryTokenType, Func<Expression, Expression>> UnaryTokenMapping { get; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryCompile(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (expression is IUnaryExpressionNode unaryExpressionNode && UnaryTokenMapping.TryGetValue(unaryExpressionNode.Token, out var func))
                return func(context.Compile(unaryExpressionNode.Operand));
            return null;
        }

        #endregion
    }
}