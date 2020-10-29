using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class UnaryExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        #region Constructors

        public UnaryExpressionConverter()
        {
            Mapping = new Dictionary<ExpressionType, Func<UnaryExpression, UnaryTokenType?>>(7)
            {
                {ExpressionType.Negate, _ => UnaryTokenType.Minus},
                {ExpressionType.UnaryPlus, _ => UnaryTokenType.Plus},
                {ExpressionType.Not, expression => expression.Type == typeof(bool) ? UnaryTokenType.LogicalNegation : UnaryTokenType.BitwiseNegation},
                {ExpressionType.Convert, _ => null},
                {ExpressionType.ConvertChecked, _ => null}
            };
        }

        #endregion

        #region Properties

        public Dictionary<ExpressionType, Func<UnaryExpression, UnaryTokenType?>> Mapping { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Unary;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is UnaryExpression unaryExpression && Mapping.TryGetValue(unaryExpression.NodeType, out var func))
            {
                var type = func(unaryExpression);
                if (type == null)
                    return context.Convert(unaryExpression.Operand);
                return UnaryExpressionNode.Get(type, context.Convert(unaryExpression.Operand));
            }

            return null;
        }

        #endregion
    }
}