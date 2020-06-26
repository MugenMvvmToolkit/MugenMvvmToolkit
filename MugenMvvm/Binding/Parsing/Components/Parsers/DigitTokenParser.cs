using System;
using System.Collections.Generic;
using System.Globalization;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components.Parsers
{
    public sealed class DigitTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Constructors

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

        #endregion

        #region Properties

        public Dictionary<string, ConvertDelegate> PostfixToConverter { get; }

        public int Priority { get; set; } = ParsingComponentPriority.Constant;

        #endregion

        #region Implementation of interfaces

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
                var result = convert(value, integer, postfix, context);
                if (result != null)
                {
                    context.Position = end;
                    return result;
                }
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseDigitExpressionFormat1.Format(value.ToString() + postfix));
            return null;
        }

        #endregion

        #region Methods

#if SPAN_API
        public static IExpressionNode? Convert(ReadOnlySpan<char> value, bool integer, string postfix, ITokenParserContext context)
#else
        public static IExpressionNode? Convert(string value, bool integer, string postfix, ITokenParserContext context)
#endif
        {
            switch (postfix)
            {
                case "f":
                case "F":
                    if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                        return new ConstantExpressionNode(f, typeof(float));
                    return null;
                case "d":
                case "D":
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        return new ConstantExpressionNode(d, typeof(double));
                    return null;
                case "m":
                case "M":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var m))
                        return new ConstantExpressionNode(m, typeof(decimal));
                    return null;
                case "u":
                case "U":
                    if (uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ui))
                        return ConstantExpressionNode.Get(ui);
                    return null;
                case "ul":
                case "UL":
                case "Ul":
                case "uL":
                    if (ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ul))
                        return ConstantExpressionNode.Get(ul);
                    return null;
                case "":
                    if (integer)
                    {
                        if (ulong.TryParse(value, out var result))
                        {
                            if (result <= int.MaxValue)
                                return ConstantExpressionNode.Get((int)result);
                            if (result <= long.MaxValue)
                                return ConstantExpressionNode.Get((long)result);
                            return ConstantExpressionNode.Get(result);
                        }

                        return null;
                    }

                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
                        return new ConstantExpressionNode(r, typeof(double));
                    return null;
            }

            return null;
        }

        #endregion

        #region Nested types

#if SPAN_API
        public delegate IExpressionNode? ConvertDelegate(ReadOnlySpan<char> value, bool integer, string postfix, ITokenParserContext context);
#else
        public delegate IExpressionNode? ConvertDelegate(string value, bool integer, string postfix, ITokenParserContext context);
#endif

        #endregion
    }
}