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

        public int Priority { get; set; } = ParserComponentPriority.Unary;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var position = context.SkipWhitespacesPosition();
            if (context.IsEof(position) || !TokensMapping.TryGetValue(MugenBindingExtensions.TokenAt(context, position), out var values))
                return null;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (context.IsToken(value.Value, position))
                {
                    context.Position = position + value.Value.Length;
                    if (value == UnaryTokenType.DynamicExpression || value == UnaryTokenType.StaticExpression)
                        return new UnaryExpressionNode(value, context.Parse());

                    IExpressionNode? operand = null;
                    while (true)
                    {
                        operand = context.Parse(operand);
                        var p = context.SkipWhitespacesPosition();
                        if (!context.IsToken('.', p) && !context.IsToken('[', p))
                            break;
                    }
                    return new UnaryExpressionNode(value, operand);
                }
            }

            return null;
        }

        #endregion
    }
}