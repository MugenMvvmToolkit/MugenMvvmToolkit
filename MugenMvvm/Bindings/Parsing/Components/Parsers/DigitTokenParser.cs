using System;
using System.Collections.Generic;
using System.Globalization;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class DigitTokenParser : ITokenParserComponent, IHasPriority
    {
        public DigitTokenParser()
        {
            ConvertDelegate defaultConverter = Convert;
            PostfixToConverter = new Dictionary<string, ConvertDelegate>(17, StringComparer.Ordinal)
            {
                [""] = defaultConverter,
                ["f"] = defaultConverter,
                ["F"] = defaultConverter,
                ["d"] = defaultConverter,
                ["D"] = defaultConverter,
                ["m"] = defaultConverter,
                ["M"] = defaultConverter,
                ["u"] = defaultConverter,
                ["U"] = defaultConverter,
                ["ul"] = defaultConverter,
                ["UL"] = defaultConverter,
                ["Ul"] = defaultConverter,
                ["uL"] = defaultConverter
            };
        }

        public Dictionary<string, ConvertDelegate> PostfixToConverter { get; }

        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var start = context.SkipWhitespacesPosition();
            if (!context.IsDigit(start))
                return null;

            var integer = true;
            var end = start;
            do
            {
                ++end;
            } while (context.IsDigit(end));

            //1.1
            if (context.IsToken('.', end) && context.IsDigit(end + 1))
            {
                integer = false;
                do
                {
                    ++end;
                } while (context.IsDigit(end));
            }

            //1e-1
            if (context.IsToken('e', end) || context.IsToken('E', end))
            {
                integer = false;
                ++end;
                if (context.IsToken('+', end) || context.IsToken('-', end))
                    ++end;

                if (!context.IsDigit(end))
                {
                    if (!context.IsEof(end))
                        ++end;
                    context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseDigitExpressionFormat1.Format(context.GetValue(start, end)));
                    return null;
                }

                do
                {
                    ++end;
                } while (context.IsDigit(end));
            }

#if SPAN_API
            var value = context.GetValueSpan(start, end);
#else
            var value = context.GetValue(start, end);
#endif
            var postfix = "";
            if (context.IsIdentifier(out var position, end))
            {
                postfix = context.GetValue(end, position);
                end = position;
            }

            if (PostfixToConverter.TryGetValue(postfix, out var convert))
            {
                var result = convert(value, integer, postfix, context, FormatProvider);
                if (result != null)
                {
                    context.Position = end;
                    return result;
                }
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseDigitExpressionFormat1.Format(value.ToString() + postfix));
            return null;
        }

#if SPAN_API
        public static IExpressionNode? Convert(ReadOnlySpan<char> value, bool integer, string postfix, ITokenParserContext context, IFormatProvider formatProvider)
#else
        public static IExpressionNode? Convert(string value, bool integer, string postfix, ITokenParserContext context, IFormatProvider formatProvider)
#endif
        {
            switch (postfix)
            {
                case "f":
                case "F":
                    if (float.TryParse(value, NumberStyles.Any, formatProvider, out var f))
                        return new ConstantExpressionNode(f, typeof(float));
                    return null;
                case "d":
                case "D":
                    if (double.TryParse(value, NumberStyles.Any, formatProvider, out var d))
                        return new ConstantExpressionNode(d, typeof(double));
                    return null;
                case "m":
                case "M":
                    if (decimal.TryParse(value, NumberStyles.Any, formatProvider, out var m))
                        return new ConstantExpressionNode(m, typeof(decimal));
                    return null;
                case "u":
                case "U":
                    if (uint.TryParse(value, NumberStyles.Any, formatProvider, out var ui))
                        return ConstantExpressionNode.Get(ui);
                    return null;
                case "ul":
                case "UL":
                case "Ul":
                case "uL":
                    if (ulong.TryParse(value, NumberStyles.Any, formatProvider, out var ul))
                        return ConstantExpressionNode.Get(ul);
                    return null;
                case "":
                    if (integer)
                    {
                        if (ulong.TryParse(value, NumberStyles.Any, formatProvider, out var result))
                        {
                            if (result <= int.MaxValue)
                                return ConstantExpressionNode.Get((int) result);
                            if (result <= long.MaxValue)
                                return ConstantExpressionNode.Get((long) result);
                            return ConstantExpressionNode.Get(result);
                        }

                        return null;
                    }

                    if (double.TryParse(value, NumberStyles.Any, formatProvider, out var r))
                        return new ConstantExpressionNode(r, typeof(double));
                    return null;
            }

            return null;
        }

#if SPAN_API
        public delegate IExpressionNode? ConvertDelegate(ReadOnlySpan<char> value, bool integer, string postfix, ITokenParserContext context, IFormatProvider formatProvider);
#else
        public delegate IExpressionNode? ConvertDelegate(string value, bool integer, string postfix, ITokenParserContext context, IFormatProvider formatProvider);
#endif
    }
}