using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    //https://devblogs.microsoft.com/csharpfaq/what-character-escape-sequences-are-available/
    //note doesn't support unicode escape sequence
    public sealed class StringExpressionParserComponent : IExpressionParserComponent<ITokenExpressionParserContext>, IHasPriority
    {
        #region Fields

        private readonly Dictionary<char, char> _escapeSequenceMap;

        private readonly IReadOnlyList<string> _quoteTokens;

        public static readonly ConstantExpressionNode Empty = new ConstantExpressionNode("", typeof(string));
        public static readonly ConstantExpressionNode StringType = new ConstantExpressionNode(typeof(string), typeof(Type));

        #endregion

        #region Constructors

        public StringExpressionParserComponent(IReadOnlyList<string>? quoteTokens = null, Dictionary<char, char>? escapeSequenceMap = null)
        {
            if (quoteTokens == null)
            {
                _quoteTokens = new[]
                {
                    "&amp;",
                    "\"",
                    "'"
                };
            }
            else
                _quoteTokens = quoteTokens;

            if (escapeSequenceMap == null)
            {
                _escapeSequenceMap = new Dictionary<char, char>
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
            else
                _escapeSequenceMap = escapeSequenceMap;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private IExpressionNode? TryParseInternal(ITokenExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
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
            List<IExpressionNode>? args = null;
            StringBuilder? builder = null;
            var start = context.Position;
            int? end;
            while (true)
            {
                if (!isVerbatim && context.IsToken('\\'))
                {
                    int prevEnd = context.Position;
                    context.MoveNext();
                    char result;
                    var token = GetQuoteToken(context);
                    if (token == null)
                    {
                        if (_escapeSequenceMap.TryGetValue(context.TokenAt(), out result))
                            context.MoveNext();
                        else
                            return null;
                    }
                    else
                    {
                        context.MoveNext(token.Length);
                        result = '"';
                    }


                    InitializeBuilder(context, start, prevEnd, ref builder);
                    builder!.Append(result);
                    continue;
                }

                if (isInterpolated && context.IsToken('{'))
                {
                    int intStart = context.Position;
                    if (context.MoveNext().IsToken('{'))
                    {
                        InitializeBuilder(context, start, context.Position, ref builder);
                        context.MoveNext();
                        continue;
                    }

                    var node = context.TryParseWhileNotNull(null, metadata);
                    if (node == null)
                        return null;

                    context.SkipWhitespaces();
                    if (!context.IsToken(':') && !context.IsToken('}'))
                        return null;

                    if (args == null)
                        args = new List<IExpressionNode>();

                    InitializeBuilder(context, start, intStart, ref builder);
                    builder!.Append('{').Append(args.Count.ToString(CultureInfo.InvariantCulture));
                    args.Add(node);

                    if (context.IsToken(':'))
                    {
                        while (!context.IsEof() && !context.IsToken('}'))
                        {
                            builder.Append(context.TokenAt());
                            context.MoveNext();
                        }
                    }

                    if (!context.IsToken('}'))
                        return null;
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
                            builder!.Append('"');
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
                    return Empty;
                return new ConstantExpressionNode(context.GetValue(start, end.Value), typeof(string));
            }

            var st = builder.ToString();
            if (args == null)
                return new ConstantExpressionNode(st, typeof(string));
            args.Insert(0, new ConstantExpressionNode(st, typeof(string)));
            return new MethodCallExpressionNode(StringType, "Format", args);
        }

        private static void InitializeBuilder(ITokenExpressionParserContext context, int start, int end, ref StringBuilder? builder)
        {
            if (builder != null)
                return;
            builder = new StringBuilder(context.GetValue(start, end));
        }

        private string? GetQuoteToken(ITokenExpressionParserContext context)
        {
            if (context.IsEof())
                return null;
            for (var i = 0; i < _quoteTokens.Count; i++)
            {
                if (context.IsToken(_quoteTokens[i]))
                    return _quoteTokens[i];
            }

            return null;
        }

        #endregion
    }
}