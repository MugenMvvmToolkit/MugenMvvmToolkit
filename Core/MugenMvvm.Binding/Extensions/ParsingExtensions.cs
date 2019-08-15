using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static int GetPosition(this IBindingParserContext context, int? position)
        {
            return position.GetValueOrDefault(context.Position);
        }

        public static char TokenAt(this IBindingParserContext context, int? position)
        {
            return context.TokenAt(context.GetPosition(position));
        }

        public static bool IsEof(this IBindingParserContext context, int? position)
        {
            return context.GetPosition(position) >= context.Length;
        }

        public static bool IsToken(this IBindingParserContext context, char token, int? position)
        {
            if (context.IsEof(position))
                return false;
            return context.TokenAt(position) == token;
        }

        public static bool IsToken(this IBindingParserContext context, string token, int? position)
        {
            var p = context.GetPosition(position);
            var i = 0;
            while (i != token.Length)
            {
                var pos = p + i;
                if (context.IsEof(pos) || TokenAt(context, pos) != token[i])
                    return false;
                ++i;
            }

            return true;
        }

        public static bool IsAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position)
        {
            if (context.IsEof(position))
                return false;
            return tokens.Contains(context.TokenAt(position));
        }

        public static bool IsAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position)
        {
            if (context.IsEof(position))
                return false;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (context.IsToken(tokens[i], position))
                    return true;
            }

            return false;
        }

        public static bool IsEofOrAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsEofOrAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsIdentifier(this IBindingParserContext context, int? position, out int endPosition)
        {
            endPosition = context.GetPosition(position);
            if (context.IsEof(endPosition) || !IsValidIdentifierSymbol(true, TokenAt(context, endPosition)))
                return false;

            do
            {
                ++endPosition;
            } while (!context.IsEof(endPosition) && IsValidIdentifierSymbol(false, TokenAt(context, endPosition)));

            return true;
        }

        public static int FindToken(this IBindingParserContext context, int? position, char token)
        {
            var start = context.GetPosition(position);
            for (int i = start; i < context.Length; i++)
            {
                if (TokenAt(context, i) == token)
                    return i;
            }

            return -1;
        }

        public static int FindAnyOf(this IBindingParserContext context, int? position, HashSet<char> tokens)
        {
            var start = context.GetPosition(position);
            for (int i = start; i < context.Length; i++)
            {
                if (tokens.Contains(context.TokenAt(i)))
                    return i;
            }

            return -1;
        }

        public static int SkipWhitespaces(this IBindingParserContext context)
        {
            var position = context.Position;
            while (!context.IsEof(position) && char.IsWhiteSpace(TokenAt(context, position)))
                ++position;
            return position;
        }

        public static void MoveNext(this IBindingParserContext context)
        {
            if (!context.IsEof(context.Position))
                context.SetPosition(context.Position + 1);
        }

        public static IExpressionNode Parse(this IBindingParserContext context, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            var node = context.TryParse(expression, metadata);
            if (node == null)
                throw new Exception();//todo add
            return node;
        }

        public static IExpressionNode? TryParseWhileNotNull(this IBindingParserContext context, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            IExpressionNode? result = expression;
            while (true)
            {
                var node = context.TryParse(result, metadata);
                if (node == null)
                    return result;
                result = node;
            }
        }

        public static IExpressionNode? ParseWhileToken(this IBindingParserContext context, char token, int? position = null, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsToken(token, position) && !context.IsEof(position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        public static IExpressionNode? ParseWhileAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position = null, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        public static IExpressionNode? ParseWhileAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position = null, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        private static bool IsValidIdentifierSymbol(bool firstSymbol, char symbol)
        {
            if (firstSymbol)
                return char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return char.IsLetterOrDigit(symbol) || symbol == '_';
        }

        #endregion
    }
}