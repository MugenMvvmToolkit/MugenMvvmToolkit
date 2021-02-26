using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    //https://devblogs.microsoft.com/csharpfaq/what-character-escape-sequences-are-available/
    //note doesn't support unicode escape sequence
    public sealed class StringTokenParser : ITokenParserComponent, IHasPriority
    {
        public StringTokenParser()
        {
            QuoteTokens = new List<string>(3)
            {
                "&amp;",
                "\"",
                "'"
            };

            EscapeSequenceMap = new Dictionary<char, char>(11)
            {
                {'\\', '\\'},
                {'0', '\0'},
                {'a', '\a'},
                {'b', '\b'},
                {'f', '\f'},
                {'n', '\n'},
                {'r', '\r'},
                {'t', '\t'},
                {'v', '\v'}
            };
        }

        public Dictionary<char, char> EscapeSequenceMap { get; }

        public List<string> QuoteTokens { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        private static void AddErrorIfNeed(string message, ITokenParserContext context, int start, int end, ref StringBuilder? builder, object? param = null)
        {
            var errors = context.TryGetErrors();
            if (errors != null)
            {
                if (start < end)
                    InitializeBuilder(context, start, end, ref builder);
                errors.Add(message.Format(builder, param));
            }
        }

        private static void InitializeBuilder(ITokenParserContext context, int start, int end, [NotNull] ref StringBuilder? builder)
        {
#if SPAN_API
            builder ??= new StringBuilder().Append(context.GetValueSpan(start, end));
#else
            builder ??= new StringBuilder(context.GetValue(start, end));
#endif
        }

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        private IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            context.SkipWhitespaces();
            var isInterpolated = context.IsToken('$');
            if (isInterpolated)
                context.MoveNext();
            var isVerbatim = context.IsToken('@');
            if (isVerbatim)
                context.MoveNext();
            if (!isInterpolated)
            {
                isInterpolated = context.IsToken('$');
                if (isInterpolated)
                    context.MoveNext();
            }

            var quoteToken = GetQuoteToken(context);
            if (quoteToken == null)
                return null;

            context.MoveNext(quoteToken.Length);
            var args = new ItemOrListEditor<IExpressionNode>();
            StringBuilder? builder = null;

            var openedBraceCount = 0;
            var start = context.Position;
            int? end;
            while (true)
            {
                if (context.IsEof())
                {
                    AddErrorIfNeed(BindingMessageConstant.CannotParseStringExpressionExpectedTokenFormat2, context, start, context.Position, ref builder, quoteToken);
                    return null;
                }

                if (!isVerbatim && context.IsToken('\\'))
                {
                    var prevEnd = context.Position;
                    context.MoveNext();
                    char result;
                    var token = GetQuoteToken(context);
                    if (token == null)
                    {
                        if (EscapeSequenceMap.TryGetValue(context.TokenAt(), out result))
                            context.MoveNext();
                        else
                        {
                            AddErrorIfNeed(BindingMessageConstant.CannotParseStringExpressionInvalidEscapeSequenceFormat2, context, start, context.Position, ref builder,
                                context.TokenAt());
                            return null;
                        }
                    }
                    else
                    {
                        context.MoveNext(token.Length);
                        result = '"';
                    }


                    InitializeBuilder(context, start, prevEnd, ref builder);
                    builder.Append(result);
                    continue;
                }

                if (isInterpolated && context.IsToken('{'))
                {
                    var intStart = context.Position;
                    if (context.MoveNext().IsToken('{'))
                    {
                        InitializeBuilder(context, start, context.Position, ref builder);
                        builder.Append("{");
                        ++openedBraceCount;
                        context.MoveNext();
                        continue;
                    }

                    var node = context.TryParseWhileNotNull();
                    if (node == null)
                    {
                        AddErrorIfNeed(BindingMessageConstant.CannotParseInterpolatedStringExpressionExpectedExpressionFormat1, context, start, intStart, ref builder);
                        return null;
                    }

                    context.SkipWhitespaces();
                    if (!context.IsToken(':') && !context.IsToken('}'))
                    {
                        AddErrorIfNeed(BindingMessageConstant.CannotParseInterpolatedStringExpressionExpectedTokensFormat1, context, start, intStart, ref builder);
                        return null;
                    }

                    InitializeBuilder(context, start, intStart, ref builder);
                    builder.Append('{').Append(args.Count.ToString(CultureInfo.InvariantCulture));
                    args.Add(node);

                    if (context.IsToken(':'))
                    {
                        var count = builder.Length + 1;
                        while (!context.IsEof() && !context.IsToken('}'))
                        {
                            builder.Append(context.TokenAt());
                            context.MoveNext();
                        }

                        if (count == builder.Length)
                        {
                            AddErrorIfNeed(BindingMessageConstant.CannotParseInterpolatedStringExpressionEmptyFormatFormat1, context, start, intStart, ref builder);
                            return null;
                        }
                    }

                    if (!context.IsToken('}'))
                    {
                        AddErrorIfNeed(BindingMessageConstant.CannotParseInterpolatedStringExpressionExpectedTokenFormat1, context, start, intStart, ref builder);
                        return null;
                    }

                    builder.Append('}');
                    context.MoveNext();
                    continue;
                }

                var endToken = GetQuoteToken(context);
                if (endToken != null)
                {
                    var endPos = context.Position;
                    context.MoveNext(endToken.Length);
                    if (isVerbatim)
                    {
                        var token = GetQuoteToken(context);
                        if (token != null)
                        {
                            context.MoveNext(token.Length);
                            InitializeBuilder(context, start, endPos, ref builder);
                            builder.Append('"');
                            continue;
                        }
                    }

                    end = endPos;
                    break;
                }

                var t = context.TokenAt();
                builder?.Append(t);
                context.MoveNext();

                if (openedBraceCount != 0 && t == '}' && context.TokenAt() == '}')
                {
                    --openedBraceCount;
                    context.MoveNext();
                }
            }

            if (openedBraceCount != 0)
            {
                AddErrorIfNeed(BindingMessageConstant.CannotParseInterpolatedStringExpressionExpectedTokenFormat1, context, start, context.Position, ref builder);
                return null;
            }

            if (builder == null)
            {
                if (start == end.Value)
                    return ConstantExpressionNode.EmptyString;
                return new ConstantExpressionNode(context.GetValue(start, end.Value), typeof(string));
            }

            var st = builder.ToString();
            var value = new ConstantExpressionNode(st, typeof(string));
            if (args.IsEmpty)
                return value;

            if (args.Count == 1)
                return new MethodCallExpressionNode(TypeAccessExpressionNode.Get<string>(), nameof(string.Format), new[] {value, args[0]});

            var list = args.AsList();
            list.Insert(0, value);
            return new MethodCallExpressionNode(TypeAccessExpressionNode.Get<string>(), nameof(string.Format), new ItemOrIReadOnlyList<IExpressionNode>((IReadOnlyList<IExpressionNode>) list));
        }

        private string? GetQuoteToken(ITokenParserContext context)
        {
            if (context.IsEof())
                return null;
            for (var i = 0; i < QuoteTokens.Count; i++)
            {
                if (context.IsToken(QuoteTokens[i]))
                    return QuoteTokens[i];
            }

            return null;
        }
    }
}