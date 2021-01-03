using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        #region Fields

        private static readonly HashSet<char> BindingTargetDelimiters = new() {',', ';', ' '};
        private static readonly HashSet<char> BindingDelimiters = new() {',', ';'};

        #endregion

        #region Methods

        public static EnumFlags<T> GetFlags<T>(this IExpressionNode expression, string key, EnumFlags<T> defaultFlags) where T : class, IFlagsEnum
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(key, nameof(key));
            EnumFlags<T> flags = default;
            while (expression != null)
            {
                flags |= expression.TryGetMetadataValue(key, default(EnumFlags<T>));
                expression = (expression as IHasTargetExpressionNode<IExpressionNode>)?.Target!;
            }

            if (flags.Flags == 0)
                return defaultFlags;
            return flags;
        }

        [return: MaybeNull]
        public static TValue TryGetMetadataValue<TValue>(this IExpressionNode expression, string key, TValue defaultValue = default)
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(key, nameof(key));
            if (expression.Metadata.TryGetValue(key, out var v))
                return (TValue) v!;
            return defaultValue;
        }

        public static bool MetadataEquals(this IExpressionNode expression, IReadOnlyDictionary<string, object?> otherMetadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            var metadata = expression.Metadata;
            if (ReferenceEquals(metadata, otherMetadata))
                return true;
            if (metadata.Count != otherMetadata.Count)
                return false;

            foreach (var pair in metadata)
            {
                if (!otherMetadata.TryGetValue(pair.Key, out var v) || !Equals(v, pair.Value))
                    return false;
            }

            return true;
        }

        public static bool TryConvertExtension(this IExpressionConverterContext<Expression> context, MemberInfo member, Expression? expression, out IExpressionNode? result)
        {
            var attribute = BindingSyntaxExtensionAttributeBase.TryGet(member);
            if (attribute != null)
                return attribute.TryConvert(context, expression, out result);
            result = null;
            return false;
        }

        public static IExpressionNode? ConvertTarget(this IExpressionConverterContext<Expression> context, Expression? expression, MemberInfo member)
        {
            if (!context.TryConvertExtension(member.DeclaringType ?? typeof(object), expression, out var result))
                result = context.ConvertOptional(expression) ?? ConstantExpressionNode.Get(member.DeclaringType);
            if (ReferenceEquals(result, ConstantExpressionNode.Null) || ReferenceEquals(result, MemberExpressionNode.Empty))
                result = null;
            return result;
        }

        [return: NotNullIfNotNull("expression")]
        public static IExpressionNode? ConvertOptional(this IExpressionConverterContext<Expression> context, Expression? expression) => expression == null ? null : context.Convert(expression);

        [return: NotNullIfNotNull("expression")]
        public static List<IExpressionNode> Convert(this IExpressionConverterContext<Expression> context, IReadOnlyList<Expression> expressions)
        {
            var nodes = new List<IExpressionNode>(expressions.Count);
            for (var i = 0; i < expressions.Count; i++)
                nodes.Add(context.Convert(expressions[i]));
            return nodes;
        }

        public static IMethodCallExpressionNode ConvertMethodCall(this IExpressionConverterContext<Expression> context, MethodCallExpression methodCallExpression, string? methodName = null)
        {
            var method = methodCallExpression.Method;
            ParameterInfo[]? parameters = null;
            IExpressionNode? target;
            var args = context.Convert(methodCallExpression.Arguments);
            if (method.GetAccessModifiers(true, ref parameters).HasFlag(MemberFlags.Extension))
            {
                target = args[0];
                args.RemoveAt(0);
            }
            else
                target = context.ConvertTarget(methodCallExpression.Object, method);

            string[]? typeArgs = null;
            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();
                typeArgs = new string[genericArguments.Length];
                for (var i = 0; i < typeArgs.Length; i++)
                    typeArgs[i] = genericArguments[i].AssemblyQualifiedName!;
            }

            return new MethodCallExpressionNode(target, methodName ?? method.Name, args, typeArgs);
        }

        public static IExpressionNode Convert<T>(this IExpressionConverterContext<T> context, T expression) where T : class
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(expression, nameof(expression));
            var exp = context.TryConvert(expression);
            if (exp != null)
                return exp;

            context.ThrowCannotParse(expression);
            return null;
        }

        [DoesNotReturn]
        public static void ThrowCannotParse<T>(this IParserContext context, T expression)
        {
            var errors = context.TryGetErrors();
            if (errors != null && errors.Count != 0)
            {
                errors.Reverse();
                ExceptionManager.ThrowCannotParseExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                ExceptionManager.ThrowCannotParseExpression(expression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<string>? TryGetErrors(this IParserContext context) => context.GetOrDefault(ParsingMetadata.ParsingErrors, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPosition(this ITokenParserContext context, int? position = null)
        {
            Should.NotBeNull(context, nameof(context));
            if (position == null)
                return context.Position;
            return position.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char TokenAt(this ITokenParserContext context) => context.TokenAt(context.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char TokenAt(this ITokenParserContext context, int position) => context.TokenAt(position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char TokenAt(this ITokenParserContext context, int? position) => context.TokenAt(context.GetPosition(position));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEof(this ITokenParserContext context) => context.Position >= context.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEof(this ITokenParserContext context, int position) => position >= context.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEof(this ITokenParserContext context, int? position) => context.GetPosition(position) >= context.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToken(this ITokenParserContext context, char token) => context.IsToken(token, context.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToken(this ITokenParserContext context, char token, int position) => !context.IsEof(position) && context.TokenAt(position) == token;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToken(this ITokenParserContext context, char token, int? position) => !context.IsEof(position) && context.TokenAt(position) == token;

        public static bool IsToken(this ITokenParserContext context, string token, int? position = null, bool isPartOfIdentifier = true)
        {
            var length = token.Length;
            if (length == 1)
                return context.IsToken(token[0], position);

            var p = context.GetPosition(position);
            var i = 0;
            var ctxLength = context.Length;
            while (i != length)
            {
                if (p >= ctxLength || context.TokenAt(p) != token[i])
                    return false;
                ++i;
                ++p;
            }

            return isPartOfIdentifier || p >= ctxLength || !context.TokenAt(p).IsValidIdentifierSymbol(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null) => !context.IsEof(position) && tokens.Contains(context.TokenAt(position));

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

        public static bool IsEofOrAnyOf(this ITokenParserContext context, HashSet<char>? tokens, int? position = null) => context.IsEof(position) || tokens != null && context.IsAnyOf(tokens, position);

        public static bool IsEofOrAnyOf(this ITokenParserContext context, IReadOnlyList<string> tokens, int? position = null) => context.IsEof(position) || context.IsAnyOf(tokens, position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIdentifier(this ITokenParserContext context, out int endPosition, int? position = null) => context.IsIdentifier(out endPosition, context.GetPosition(position));

        public static bool IsIdentifier(this ITokenParserContext context, out int endPosition, int position)
        {
            endPosition = position;
            if (context.IsEof(endPosition) || !TokenAt(context, endPosition).IsValidIdentifierSymbol(true))
                return false;

            do
            {
                ++endPosition;
            } while (!context.IsEof(endPosition) && TokenAt(context, endPosition).IsValidIdentifierSymbol(false));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(this ITokenParserContext context, int? position = null) => !context.IsEof(position) && char.IsDigit(context.TokenAt(position));

        public static int FindAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            var start = context.GetPosition(position);
            var length = context.Length;
            for (var i = start; i < length; i++)
            {
                if (tokens.Contains(context.TokenAt(i)))
                    return i;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SkipWhitespacesPosition(this ITokenParserContext context) => context.SkipWhitespacesPosition(context.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SkipWhitespacesPosition(this ITokenParserContext context, int position)
        {
            while (!context.IsEof(position) && char.IsWhiteSpace(TokenAt(context, position)))
                ++position;
            return position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITokenParserContext SkipWhitespaces(this ITokenParserContext context) => context.SkipWhitespaces(context.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITokenParserContext SkipWhitespaces(this ITokenParserContext context, int position)
        {
            context.Position = context.SkipWhitespacesPosition(position);
            return context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITokenParserContext MoveNext(this ITokenParserContext context, int value = 1)
        {
            if (!context.IsEof())
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

        public static IExpressionNode ParseWhileAnyOf(this ITokenParserContext context, HashSet<char>? tokens, int? position = null, IExpressionNode? expression = null)
        {
            var expressionNode = context.Parse(expression);
            while (!context.SkipWhitespaces().IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode);
            return expressionNode;
        }

        public static List<IExpressionNode>? ParseArguments(this ITokenParserContext context, string endSymbol)
        {
            LazyList<IExpressionNode> args = default;
            while (true)
            {
                var node = context.TryParseWhileNotNull();
                if (node != null)
                {
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
                    ? BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(args.List == null ? null : string.Join(",", args))
                    : BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(string.Join(",", args), endSymbol));

                return null;
            }

            return args;
        }

        public static string[]? ParseStringArguments(this ITokenParserContext context, string endSymbol, bool isPointSupported)
        {
            LazyList<(int start, int end)> args = default;
            var start = context.Position;
            int? end = null;
            while (true)
            {
                var isEnd = context.SkipWhitespaces().IsToken(endSymbol);
                if (isEnd || context.IsToken(','))
                {
                    if (end == null)
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(context.Format(args)));
                        return null;
                    }

                    args.Add((start, end.Value));
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

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(context.Format(args), endSymbol));
                return null;
            }

            var list = args.List;
            if (list == null)
                return null;
            var result = new string[list.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var t = list[i];
                result[i] = context.GetValue(t.start, t.end);
            }

            return result;
        }

        public static ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseExpression(this ITokenParserContext context)
        {
            var result = ItemOrListEditor.Get<ExpressionParserResult>();
            while (!context.IsEof())
            {
                var r = TryParseNext(context);
                if (!r.IsEmpty)
                    result.Add(r);
                context.SkipWhitespaces();
            }

            return result.ToItemOrList<IReadOnlyList<ExpressionParserResult>>();
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

            var parameters = ItemOrListEditor.Get<IExpressionNode>();
            while (context.IsToken(','))
            {
                parameters.Add(context.MoveNext().ParseWhileAnyOf(BindingDelimiters));
                errors?.Clear();
            }

            if (context.SkipWhitespaces().IsEof() || context.IsToken(';'))
            {
                if (context.IsToken(';'))
                    context.MoveNext();
                if (isActionToken)
                    return new ExpressionParserResult(UnaryExpressionNode.ActionMacros, target, parameters.ToItemOrList<IReadOnlyList<IExpressionNode>>());
                return new ExpressionParserResult(target, source ?? MemberExpressionNode.Empty, parameters.ToItemOrList<IReadOnlyList<IExpressionNode>>());
            }

            context.ThrowCannotParse(context);
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidIdentifierSymbol(this char symbol, bool isFirstSymbol)
        {
            if (isFirstSymbol)
                return char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return char.IsLetterOrDigit(symbol) || symbol == '_';
        }

        private static string? Format(this ITokenParserContext context, LazyList<(int start, int end)> args) =>
            args.List == null ? null : string.Join(",", args.List.Select(tuple => context.GetValue(tuple.start, tuple.end)));

        #endregion
    }
}