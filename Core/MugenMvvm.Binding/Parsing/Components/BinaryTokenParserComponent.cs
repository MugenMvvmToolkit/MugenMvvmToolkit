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
    public sealed class BinaryTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Fields

        private readonly BinaryTokenType[] _tokens;
        private readonly List<KeyValuePair<List<IExpressionNode>, List<BinaryTokenType>>> _buffers;
        private int _nestedIndex;

        #endregion

        #region Constructors

        public BinaryTokenParserComponent(BinaryTokenType[]? mapping = null)
        {
            _nestedIndex = -1;
            _buffers = new List<KeyValuePair<List<IExpressionNode>, List<BinaryTokenType>>>();
            if (mapping == null)
            {
                _tokens = new[]
                {
                    BinaryTokenType.Multiplication,
                    BinaryTokenType.Division,
                    BinaryTokenType.Remainder,
                    BinaryTokenType.Addition,
                    BinaryTokenType.Subtraction,
                    BinaryTokenType.LeftShift,
                    BinaryTokenType.RightShift,
                    BinaryTokenType.LessThan,
                    BinaryTokenType.GreaterThan,
                    BinaryTokenType.LessThanOrEqual,
                    BinaryTokenType.GreaterThanOrEqual,
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

        public int Priority { get; set; } = ParsingComponentPriority.Binary;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            try
            {
                ++_nestedIndex;
                var p = context.Position;
                var node = TryParseInternal(context, expression);
                if (node == null)
                    context.Position = p;
                return node;
            }
            finally
            {
                --_nestedIndex;
            }
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression == null)
                return null;

            var token = GetToken(context.SkipWhitespaces());
            if (token == null)
                return null;

            GetBuffers(out var nodes, out var tokens);
            nodes.Add(expression);
            tokens.Add(token);

            expression = null;
            while (true)
            {
                var newNode = context.TryParse(expression, parser => parser.GetPriority() >= ParsingComponentPriority.Binary);
                if (newNode == null)
                {
                    if (expression != null)
                        nodes.Add(expression);
                    break;
                }

                expression = newNode;
                if (context.SkipWhitespaces().IsEof())
                {
                    nodes.Add(expression);
                    break;
                }

                token = GetToken(context);
                if (token != null)
                {
                    nodes.Add(expression);
                    expression = null;
                    tokens.Add(token);
                }
            }

            if (nodes.Count - 1 != tokens.Count)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseBinaryExpressionFormat2.Format(nodes[nodes.Count - 1], tokens[tokens.Count - 1]));
                return null;
            }

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

        private BinaryTokenType? GetToken(ITokenParserContext context)
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

        private void GetBuffers(out List<IExpressionNode> nodes, out List<BinaryTokenType> tokens)
        {
            if (_nestedIndex >= _buffers.Count)
                _buffers.Add(new KeyValuePair<List<IExpressionNode>, List<BinaryTokenType>>(new List<IExpressionNode>(3), new List<BinaryTokenType>(3)));
            var pair = _buffers[_nestedIndex];
            nodes = pair.Key;
            tokens = pair.Value;
            nodes.Clear();
            tokens.Clear();
        }

        #endregion
    }
}