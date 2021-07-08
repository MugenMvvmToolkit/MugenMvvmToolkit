using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class ConstantTokenParser : ITokenParserComponent, IHasPriority
    {
        public ConstantTokenParser()
        {
            LiteralToExpression = new Dictionary<string, IExpressionNode>(3, StringComparer.Ordinal)
            {
                { InternalConstant.Null, ConstantExpressionNode.Null },
                { InternalConstant.True, ConstantExpressionNode.True },
                { InternalConstant.False, ConstantExpressionNode.False }
            };
        }

        public Dictionary<string, IExpressionNode> LiteralToExpression { get; }

        public int Priority { get; init; } = ParsingComponentPriority.Constant;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var start = context.SkipWhitespacesPosition();
            foreach (var expressionNode in LiteralToExpression)
            {
                if (context.IsToken(expressionNode.Key, start, false))
                {
                    context.Position = start + expressionNode.Key.Length;
                    return expressionNode.Value;
                }
            }

            return null;
        }
    }
}