﻿using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class NullConditionalMemberTokenParser : ITokenParserComponent, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Member;

        private static IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression == null)
                return null;

            context.SkipWhitespaces();
            if (context.IsToken('?'))
            {
                var position = context.SkipWhitespacesPosition(context.Position + 1);
                if (context.IsToken('.', position) || context.IsToken('[', position))
                {
                    context.MoveNext();
                    return context.TryParse(new NullConditionalMemberExpressionNode(expression));
                }
            }

            return null;
        }

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }
    }
}