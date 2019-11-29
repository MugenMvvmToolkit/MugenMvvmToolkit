using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class UnaryExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Constructors

        public UnaryExpressionConverterParserComponent()
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

        public IExpressionNode? TryConvert(IExpressionConverterParserContext<Expression> context, Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                if (Mapping.TryGetValue(unaryExpression.NodeType, out var func))
                {
                    var type = func(unaryExpression);
                    if (type == null)
                        return context.Convert(unaryExpression.Operand);
                    return new UnaryExpressionNode(type, context.Convert(unaryExpression.Operand));
                }
            }

            return null;
        }

        #endregion
    }
}