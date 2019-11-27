using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class UnaryTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Constructors

        public UnaryTokenParserComponent(Dictionary<char, UnaryTokenType[]>? mapping = null)
        {
            if (mapping == null)
            {
                TokensMapping = new Dictionary<char, UnaryTokenType[]>
                {
                    {UnaryTokenType.Minus.Value[0], new[] {UnaryTokenType.Minus}},
                    {UnaryTokenType.Plus.Value[0], new[] {UnaryTokenType.Plus}},
                    {UnaryTokenType.BitwiseNegation.Value[0], new[] {UnaryTokenType.BitwiseNegation}},
                    {UnaryTokenType.LogicalNegation.Value[0], new[] {UnaryTokenType.LogicalNegation}},
                    {UnaryTokenType.DynamicExpression.Value[0], new[] {UnaryTokenType.StaticExpression, UnaryTokenType.DynamicExpression}}
                };
            }
            else
                TokensMapping = mapping;
        }

        #endregion

        #region Properties

        public Dictionary<char, UnaryTokenType[]> TokensMapping { get; }

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
            if (context.IsEof(position) || !TokensMapping.TryGetValue(context.TokenAt(position), out var values))
                return null;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (!context.IsToken(value.Value, position))
                    continue;

                context.Position = position + value.Value.Length;
                if (value == UnaryTokenType.DynamicExpression || value == UnaryTokenType.StaticExpression)
                {
                    var node = context.TryParse();
                    if (node == null || node is ConstantExpressionNode)
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseUnaryExpressionExpectedExpressionFormat1.Format(context.TokenAt(position)));
                        return null;
                    }
                    return new UnaryExpressionNode(value, node);
                }

                IExpressionNode? operand = null;
                while (true)
                {
                    var result = context.TryParse(operand, parser => parser.GetPriority() >= ParsingComponentPriority.Unary);
                    if (result == null)
                        break;
                    operand = result;
                }

                if (operand != null)
                    return new UnaryExpressionNode(value, operand);
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseUnaryExpressionExpectedExpressionFormat1.Format(context.TokenAt(position)));
            return null;
        }

        #endregion
    }
}