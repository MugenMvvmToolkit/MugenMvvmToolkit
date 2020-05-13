using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Parsers
{
    public sealed class UnaryTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Constructors

        public UnaryTokenParser()
        {
            Mapping = new Dictionary<char, UnaryTokenType[]>(7)
            {
                {UnaryTokenType.Minus.Value[0], new[] {UnaryTokenType.Minus}},
                {UnaryTokenType.Plus.Value[0], new[] {UnaryTokenType.Plus}},
                {UnaryTokenType.BitwiseNegation.Value[0], new[] {UnaryTokenType.BitwiseNegation}},
                {UnaryTokenType.LogicalNegation.Value[0], new[] {UnaryTokenType.LogicalNegation}},
                {UnaryTokenType.DynamicExpression.Value[0], new[] {UnaryTokenType.StaticExpression, UnaryTokenType.DynamicExpression}}
            };
        }

        #endregion

        #region Properties

        public Dictionary<char, UnaryTokenType[]> Mapping { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Unary;

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

        private IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var position = context.SkipWhitespacesPosition();
            if (context.IsEof(position) || !Mapping.TryGetValue(context.TokenAt(position), out var values))
                return null;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
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
                    var result = context.TryParse(operand, (_, parser) => parser.GetPriority() >= ParsingComponentPriority.Unary);
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

        #endregion
    }
}