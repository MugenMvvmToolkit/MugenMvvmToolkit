using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MethodCallTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Method;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        #endregion

        #region Methods

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

            List<string>? typeArgs = null;
            if (context.IsToken('<'))
            {
                typeArgs = context.MoveNext().ParseStringArguments(">", true);
                if (typeArgs == null)
                    return null;
            }


            if (!context.IsToken('('))
            {
                if (typeArgs != null)
                    context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseMethodExpressionExpectedTokenFormat1.Format($"{context.GetValue(nameStart, nameEndPos)}<{string.Join(",", typeArgs)}>"));
                return null;
            }

            if (context.MoveNext().SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), Default.EmptyArray<IExpressionNode>(), typeArgs);
            }

            var args = context.ParseArguments(")");
            if (args == null)
                return null;
            return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), args, typeArgs);
        }

        #endregion
    }
}