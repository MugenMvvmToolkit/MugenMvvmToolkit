using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    //https://devblogs.microsoft.com/csharpfaq/what-character-escape-sequences-are-available/
    //note doesn't support unicode escape sequence
    public sealed class StringTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Fields

        public static readonly ConstantExpressionNode StringType = ConstantExpressionNode.Get<string>();

        #endregion

        #region Constructors

        public StringTokenParserComponent()
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

        #endregion

        #region Properties

        public Dictionary<char, char> EscapeSequenceMap { get; }

        public List<string> QuoteTokens { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        #endregion

        #region Methods

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

            var quoteToken = GetQuoteToken(context);
            if (quoteToken == null)
                return null;

            context.MoveNext(quoteToken.Length);
            LazyList<IExpressionNode> args = default;
            StringBuilder? builder = null;
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
                            AddErrorIfNeed(BindingMessageConstant.CannotParseStringExpressionInvalidEscapeSequenceFormat2, context, start, context.Position, ref builder, context.TokenAt());
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
                        int count = builder.Length + 1;
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

                builder?.Append(context.TokenAt());
                context.MoveNext();
            }

            if (builder == null)
            {
                if (start == end.Value)
                    return ConstantExpressionNode.EmptyString;
                return new ConstantExpressionNode(context.GetValue(start, end.Value), typeof(string));
            }

            var st = builder.ToString();
            if (args.List == null)
                return new ConstantExpressionNode(st, typeof(string));
            args.List.Insert(0, new ConstantExpressionNode(st, typeof(string)));
            return new MethodCallExpressionNode(StringType, "Format", args.List);
        }

        private static void AddErrorIfNeed(string message, ITokenParserContext context, int start, int end, [NotNull] ref StringBuilder? builder, object? param = null)
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
            if (builder == null)
                builder = new StringBuilder(context.GetValue(start, end));
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

        #endregion
    }
}