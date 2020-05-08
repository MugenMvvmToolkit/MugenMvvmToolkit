using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Parsers
{
    public sealed class AssignmentTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Assignment;

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
            if (expression == null || !context.SkipWhitespaces().IsToken('='))
                return null;

            var right = context.MoveNext().TryParseWhileNotNull();
            if (right == null)
                return null;
            return new BinaryExpressionNode(BinaryTokenType.Assignment, expression, right);
        }

        #endregion
    }
}