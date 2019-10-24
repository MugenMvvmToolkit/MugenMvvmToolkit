using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class BinaryTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Fields

        private readonly BinaryTokenType[] _tokens;

        #endregion

        #region Constructors

        public BinaryTokenParserComponent(BinaryTokenType[]? mapping = null)
        {
            if (mapping == null)
            {
                _tokens = new[]
                {
                    BinaryTokenType.NullConditional,
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

        public int Priority { get; set; } = BindingParserPriority.Binary;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression == null)
                return null;

            var token = GetToken(context.SkipWhitespaces());
            if (token == null)
                return null;

            var nodes = new List<IExpressionNode> {expression};
            var tokens = new List<BinaryTokenType> {token};

            expression = null;
            while (true)
            {
                context.SkipWhitespaces();
                var newNode = context.IsToken('?') && !context.IsToken("??") ? null : context.TryParse(expression);
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

        private BinaryTokenType? GetToken(TokenExpressionParserComponent.IContext context)
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