using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        private static readonly HashSet<char> BindingTargetDelimiters = new HashSet<char> { ',', ';', ' ' };
        private static readonly HashSet<char> BindingDelimiters = new HashSet<char> { ',', ';' };

        #endregion

        #region Methods

        public static IExpressionNode ConvertTarget(this IExpressionConverterContext<Expression> context, Expression? expression, MemberInfo member)
        {
            return context.ConvertOptional(expression) ?? ConstantExpressionNode.Get(member.DeclaringType);
        }

        [return: NotNullIfNotNull("expression")]
        public static IExpressionNode? ConvertOptional(this IExpressionConverterContext<Expression> context, Expression? expression)
        {
            return expression == null ? null : context.Convert(expression);
        }

        [return: NotNullIfNotNull("expression")]
        public static List<IExpressionNode> Convert(this IExpressionConverterContext<Expression> context, IReadOnlyList<Expression> expressions)
        {
            var nodes = new List<IExpressionNode>(expressions.Count);
            for (int i = 0; i < expressions.Count; i++)
                nodes.Add(context.Convert(expressions[i]));
            return nodes;
        }

        [DoesNotReturn]
        public static void ThrowCannotParse<T>(this IParserContext context, T expression)
        {
            var errors = context.TryGetErrors();
            if (errors != null && errors.Count != 0)
            {
                errors.Reverse();
                BindingExceptionManager.ThrowCannotParseExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                BindingExceptionManager.ThrowCannotParseExpression(expression);
        }

        public static List<string>? TryGetErrors(this IParserContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (context.HasMetadata && context.Metadata.TryGet(ParsingMetadata.ParsingErrors, out var errors))
                return errors;
            return null;
        }

        public static int GetPosition(this ITokenParserContext context, int? position = null)
        {
            Should.NotBeNull(context, nameof(context));
            return position.GetValueOrDefault(context.Position);
        }

        public static char TokenAt(this ITokenParserContext context, int? position = null)
        {
            return context.TokenAt(context.GetPosition(position));
        }

        public static bool IsEof(this ITokenParserContext context, int? position = null)
        {
            return context.GetPosition(position) >= context.Length;
        }

        public static bool IsToken(this ITokenParserContext context, char token, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return context.TokenAt(position) == token;
        }

        public static bool IsToken(this ITokenParserContext context, string token, int? position = null)
        {
            if (token.Length == 1)
                return context.IsToken(token[0], position);

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

        public static bool IsAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return tokens.Contains(context.TokenAt(position));
        }

        public static bool IsAnyOf(this ITokenParserContext context, IReadOnlyList<string> tokens, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (context.IsToken(tokens[i], position))
                    return true;
            }

            return false;
        }

        public static bool IsEofOrAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsEofOrAnyOf(this ITokenParserContext context, IReadOnlyList<string> tokens, int? position = null)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsIdentifier(this ITokenParserContext context, out int endPosition, int? position = null)
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

        public static bool IsDigit(this ITokenParserContext context, int? position = null)
        {
            return !context.IsEof(position) && char.IsDigit(context.TokenAt(position));
        }

        public static int FindAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            var start = context.GetPosition(position);
            for (var i = start; i < context.Length; i++)
            {
                if (tokens.Contains(context.TokenAt(i)))
                    return i;
            }

            return -1;
        }

        public static int SkipWhitespacesPosition(this ITokenParserContext context, int? position = null)
        {
            var p = context.GetPosition(position);
            while (!context.IsEof(p) && char.IsWhiteSpace(TokenAt(context, p)))
                ++p;
            return p;
        }

        public static ITokenParserContext SkipWhitespaces(this ITokenParserContext context,
            int? position = null)
        {
            context.Position = context.SkipWhitespacesPosition(position);
            return context;
        }

        public static ITokenParserContext MoveNext(this ITokenParserContext context,
            int value = 1)
        {
            if (!context.IsEof(context.Position))
                context.Position += value;
            return context;
        }

        public static IExpressionNode Parse(this ITokenParserContext context, IExpressionNode? expression = null)
        {
            Should.NotBeNull(context, nameof(context));
            var node = context.TryParse(expression);
            if (node == null)
                context.ThrowCannotParse(context);
            return node;
        }

        public static IExpressionNode? TryParseWhileNotNull(this ITokenParserContext context, IExpressionNode? expression = null)
        {
            Should.NotBeNull(context, nameof(context));
            while (true)
            {
                var node = context.TryParse(expression);
                if (node == null)
                    return expression;
                expression = node;
            }
        }

        public static IExpressionNode ParseWhileAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null,
            IExpressionNode? expression = null)
        {
            var expressionNode = context.Parse(expression);
            while (!context.SkipWhitespaces().IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode);
            return expressionNode;
        }

        public static List<IExpressionNode>? ParseArguments(this ITokenParserContext context, string endSymbol)
        {
            List<IExpressionNode>? args = null;
            while (true)
            {
                var node = context.TryParseWhileNotNull();
                if (node != null)
                {
                    if (args == null)
                        args = new List<IExpressionNode>();
                    args.Add(node);

                    if (context.SkipWhitespaces().IsToken(','))
                    {
                        context.MoveNext();
                        continue;
                    }

                    if (context.IsToken(endSymbol))
                    {
                        context.MoveNext();
                        break;
                    }
                }

                context.TryGetErrors()?.Add(node == null
                    ? BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(args == null ? null : string.Join(",", args))
                    : BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(string.Join(",", args), endSymbol));

                return null;
            }

            return args;
        }

        public static List<string>? ParseStringArguments(this ITokenParserContext context, string endSymbol, bool isPointSupported)
        {
            List<string>? args = null;
            var start = context.Position;
            int? end = null;
            while (true)
            {
                var isEnd = context.SkipWhitespaces().IsToken(endSymbol);
                if (isEnd || context.IsToken(','))
                {
                    if (end == null)
                    {

                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(args == null ? null : string.Join(",", args)));
                        return null;
                    }

                    if (args == null)
                        args = new List<string>();
                    args.Add(context.GetValue(start, end.Value));
                    context.MoveNext();
                    if (isEnd)
                        break;

                    start = context.SkipWhitespaces().Position;
                    end = null;
                    continue;
                }

                if (isPointSupported && context.IsToken('.'))
                {
                    context.MoveNext();
                    continue;
                }

                if (context.IsIdentifier(out var position))
                {
                    end = position;
                    context.Position = position;
                    continue;
                }

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(string.Join(",", args == null ? null : string.Join(",", args)), endSymbol));
                return null;
            }

            return args;
        }

        public static ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseExpression(this ITokenParserContext context)
        {
            ExpressionParserResult itemResult = default;
            List<ExpressionParserResult>? result = null;
            while (!context.IsEof())
            {
                var r = TryParseNext(context);
                if (r.IsEmpty)
                    break;
                if (itemResult.IsEmpty)
                    itemResult = r;
                else
                {
                    if (result == null)
                        result = new List<ExpressionParserResult> { itemResult };
                    result.Add(r);
                }
            }

            if (result == null)
                return itemResult;
            return result;
        }

        private static ExpressionParserResult TryParseNext(ITokenParserContext context)
        {
            var isActionToken = context.SkipWhitespaces().IsToken('@');
            int delimiterPos;
            if (isActionToken)
            {
                context.MoveNext();
                delimiterPos = -1;
            }
            else
                delimiterPos = context.FindAnyOf(BindingTargetDelimiters);
            var oldLimit = context.Limit;
            if (delimiterPos > 0)
                context.Limit = delimiterPos;

            var errors = context.TryGetErrors();
            var target = context.ParseWhileAnyOf(BindingDelimiters);
            context.Limit = oldLimit;
            errors?.Clear();

            IExpressionNode? source = null;
            if (context.IsToken(' '))
            {
                source = context.ParseWhileAnyOf(BindingDelimiters);
                errors?.Clear();
            }

            List<IExpressionNode>? parameters = null;
            IExpressionNode? parameter = null;
            while (context.IsToken(','))
            {
                var param = context.MoveNext().ParseWhileAnyOf(BindingDelimiters);
                if (parameter == null)
                    parameter = param;
                else
                {
                    if (parameters == null)
                        parameters = new List<IExpressionNode> { parameter };
                    parameters.Add(param);
                }
                errors?.Clear();
            }

            if (context.SkipWhitespaces().IsEof() || context.IsToken(';'))
            {
                if (context.IsToken(';'))
                    context.MoveNext();
                return new ExpressionParserResult(target, source ?? MemberExpressionNode.Empty, parameters ?? new ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>>(parameter));
            }

            context.ThrowCannotParse(context);
            return default;
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