using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class UnaryExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<char, UnaryTokenType[]> _tokensMapping;

        #endregion

        #region Constructors

        public UnaryExpressionParserComponent(IDictionary<char, UnaryTokenType[]>? mapping = null)
        {
            if (mapping == null)
            {
                _tokensMapping = new Dictionary<char, UnaryTokenType[]>
                {
                    {UnaryTokenType.Minus.Value[0], new[] {UnaryTokenType.Minus}},
                    {UnaryTokenType.BitwiseNegation.Value[0], new[] {UnaryTokenType.BitwiseNegation}},
                    {UnaryTokenType.LogicalNegation.Value[0], new[] {UnaryTokenType.LogicalNegation}},
                    {UnaryTokenType.DynamicExpression.Value[0], new[] {UnaryTokenType.StaticExpression, UnaryTokenType.DynamicExpression}}
                };
            }
            else
                _tokensMapping = new Dictionary<char, UnaryTokenType[]>(mapping);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 960;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression != null)
                return null;

            var position = context.SkipWhitespaces();
            if (!_tokensMapping.TryGetValue(BindingMugenExtensions.TokenAt(context, position), out var values))
                return null;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (context.IsToken(value.Value, position))
                {
                    context.SetPosition(position + value.Value.Length);
                    return new UnaryExpressionNode(value, context.Parse(null, metadata));
                }
            }

            return null;
        }

        #endregion
    }
}