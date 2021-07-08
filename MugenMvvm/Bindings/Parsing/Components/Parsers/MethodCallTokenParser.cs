using MugenMvvm.Bindings.Constants;
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
    public sealed class MethodCallTokenParser : ITokenParserComponent, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Method;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        private static IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            context.SkipWhitespaces();
            if (expression != null)
            {
                if (!context.IsToken('.'))
                    return null;
                context.MoveNext();
            }

            if (!context.IsIdentifier(out var nameEndPos))
                return null;

            var nameStart = context.Position;
            context.Position = nameEndPos;
            context.SkipWhitespaces();

            ItemOrArray<string> typeArgs = default;
            if (context.IsToken('<'))
            {
                typeArgs = context.MoveNext().ParseStringArguments(">", true);
                if (typeArgs.IsEmpty)
                    return null;
            }

            if (!context.IsToken('('))
            {
                if (!typeArgs.IsEmpty)
                {
                    context.TryGetErrors()
                           ?.Add(BindingMessageConstant.CannotParseMethodExpressionExpectedTokenFormat1.Format(
                               $"{context.GetValue(nameStart, nameEndPos)}<{string.Join(",", typeArgs)}>"));
                }

                return null;
            }

            if (context.MoveNext().SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), default, typeArgs);
            }

            var args = context.ParseArguments(")");
            if (args.IsEmpty)
                return null;
            return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), args, typeArgs);
        }
    }
}