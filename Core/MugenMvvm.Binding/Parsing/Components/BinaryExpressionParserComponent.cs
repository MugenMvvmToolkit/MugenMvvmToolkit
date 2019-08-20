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
    public sealed class BinaryExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly BinaryTokenType[] _tokens;

        #endregion

        #region Constructors

        public BinaryExpressionParserComponent(BinaryTokenType[]? mapping = null)
        {
            if (mapping == null)
            {
                _tokens = new[]
                {
                    BinaryTokenType.NullConditionalMemberAccess,
                    BinaryTokenType.Multiplication,
                    BinaryTokenType.Division,
                    BinaryTokenType.Remainder,
                    BinaryTokenType.Addition,
                    BinaryTokenType.Subtraction,
                    BinaryTokenType.LeftShift,
                    BinaryTokenType.RightShift,
                    BinaryTokenType.LessThan,
                    BinaryTokenType.GreaterThan,
                    BinaryTokenType.LessThanOrEqualTo,
                    BinaryTokenType.GreaterThanOrEqualTo,
                    BinaryTokenType.Equality,
                    BinaryTokenType.NotEqual,
                    BinaryTokenType.LogicalAnd,
                    BinaryTokenType.LogicalXor,
                    BinaryTokenType.LogicalOr,
                    BinaryTokenType.ConditionalAnd,
                    BinaryTokenType.ConditionalOr,
                    BinaryTokenType.NullCoalescing
                };
            }
            else
                _tokens = mapping;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 970;

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

        private IExpressionNode? TryParseInternal(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression == null)
                return null;

            context.SkipWhitespacesSetPosition();
            var token = GetToken(context);
            if (token == null)
                return null;

            var nodes = new List<IExpressionNode> {expression};
            var tokens = new List<BinaryTokenType> {token};

            IExpressionNode? node = null;
            while (true)
            {
                context.SkipWhitespacesSetPosition();
                var newNode = context.IsToken('?') && !context.IsToken("??") ? null : context.TryParse(node, metadata);
                if (newNode == null)
                {
                    if (node != null)
                        nodes.Add(node);
                    break;
                }

                node = newNode;
                context.SkipWhitespacesSetPosition();
                if (context.IsEof(context.Position))
                {
                    nodes.Add(node);
                    break;
                }

                token = GetToken(context);
                if (token != null)
                {
                    nodes.Add(node);
                    node = null;
                    tokens.Add(token);
                }
            }

            if (nodes.Count - 1 != tokens.Count)
                return null;

            var index = GetMaxPriorityTokenIndex(tokens);
            while (index != -1)
            {
                token = tokens[index];
                tokens.RemoveAt(index);
                nodes[index] = new BinaryExpressionNode(token, nodes[index], nodes[index + 1]);
                nodes.RemoveAt(index + 1);
                index = GetMaxPriorityTokenIndex(tokens);
            }

            return nodes[0];
        }

        private static int GetMaxPriorityTokenIndex(List<BinaryTokenType> tokens)
        {
            if (tokens.Count == 0)
                return -1;
            if (tokens.Count == 1)
                return 0;
            var index = -1;
            var priority = int.MinValue;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Priority > priority)
                {
                    priority = tokens[i].Priority;
                    index = i;
                }
            }

            return index;
        }

        private BinaryTokenType? GetToken(IBindingParserContext context)
        {
            for (var i = 0; i < _tokens.Length; i++)
            {
                var token = _tokens[i];
                if (context.IsToken(token.Value))
                {
                    context.MoveNext(token.Value.Length);
                    return token;
                }

                if (token.Aliases == null)
                    continue;

                for (var j = 0; j < token.Aliases.Length; j++)
                {
                    if (context.IsToken(token.Aliases[j]))
                    {
                        context.MoveNext(token.Aliases[j].Length);
                        return token;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}