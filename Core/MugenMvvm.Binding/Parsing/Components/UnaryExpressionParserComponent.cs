using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class UnaryExpressionParserComponent : IExpressionParserComponent
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

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression != null)
                return null;

            context.SkipWhitespaces(out var position);
            if (!_tokensMapping.TryGetValue(context.Current(position), out var values))
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