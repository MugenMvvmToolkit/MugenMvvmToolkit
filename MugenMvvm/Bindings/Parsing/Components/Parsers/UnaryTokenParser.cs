using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class UnaryTokenParser : ITokenParserComponent, IHasPriority
    {
        public UnaryTokenParser()
        {
            Mapping = new Dictionary<char, ItemOrArray<UnaryTokenType>>(7)
            {
                { UnaryTokenType.Minus.Value[0], UnaryTokenType.Minus },
                { UnaryTokenType.Plus.Value[0], UnaryTokenType.Plus },
                { UnaryTokenType.BitwiseNegation.Value[0], UnaryTokenType.BitwiseNegation },
                { UnaryTokenType.LogicalNegation.Value[0], UnaryTokenType.LogicalNegation },
                { UnaryTokenType.DynamicExpression.Value[0], new[] { UnaryTokenType.StaticExpression, UnaryTokenType.DynamicExpression } }
            };
        }

        public Dictionary<char, ItemOrArray<UnaryTokenType>> Mapping { get; }

        public int Priority { get; init; } = ParsingComponentPriority.Unary;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        private IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var position = context.SkipWhitespacesPosition();
            if (context.IsEof(position) || !Mapping.TryGetValue(context.TokenAt(position), out var values))
                return null;

            foreach (var value in values)
            {
                if (!context.IsToken(value.Value, position))
                    continue;

                context.Position = position + value.Value.Length;
                if (value.IsSingleExpression)
                {
                    var node = context.TryParse();
                    if (node == null || node is ConstantExpressionNode)
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseUnaryExpressionExpectedExpressionFormat1.Format(context.TokenAt(position).ToString()));
                        return null;
                    }

                    return UnaryExpressionNode.Get(value, node);
                }

                IExpressionNode? operand = null;
                while (true)
                {
                    var result = context.TryParse(operand, (_, parser) => MugenExtensions.GetComponentPriority(parser) >= ParsingComponentPriority.Unary);
                    if (result == null)
                        break;
                    operand = result;
                }

                if (operand != null)
                    return UnaryExpressionNode.Get(value, operand);
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseUnaryExpressionExpectedExpressionFormat1.Format(context.TokenAt(position).ToString()));
            return null;
        }
    }
}