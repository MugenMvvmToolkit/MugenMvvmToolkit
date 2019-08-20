using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MethodCallExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = 1000;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
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
            context.SetPosition(nameEndPos);
            context.SkipWhitespaces();

            List<string>? typeArgs = null;
            if (context.IsToken('<'))
            {
                typeArgs = context.MoveNext().ParseStringArguments(">", true);
                if (typeArgs == null)
                    return null;
            }


            if (!context.IsToken('('))
                return null;

            if (context.MoveNext().SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), Default.EmptyArray<IExpressionNode>(), typeArgs);
            }

            var args = context.ParseArguments(")", metadata);
            if (args == null)
                return null;
            return new MethodCallExpressionNode(expression, context.GetValue(nameStart, nameEndPos), args, typeArgs);
        }

        #endregion
    }
}