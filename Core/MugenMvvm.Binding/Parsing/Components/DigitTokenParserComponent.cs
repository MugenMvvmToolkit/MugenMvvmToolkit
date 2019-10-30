using System.Collections.Generic;
using System.Globalization;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class DigitTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Constructors

        public DigitTokenParserComponent(Dictionary<string, ConvertDelegate>? postfixToConverters = null)
        {
            if (postfixToConverters == null)
            {
                ConvertDelegate defaultConverter = Convert;
                PostfixToConverter = new Dictionary<string, ConvertDelegate>
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
            else
                PostfixToConverter = postfixToConverters;
        }

        #endregion

        #region Properties

        public Dictionary<string, ConvertDelegate> PostfixToConverter { get; }

        public int Priority { get; set; } = BindingParserPriority.Constant;

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
                    return null;

                do
                {
                    ++end;
                } while (context.IsDigit(end));
            }

            var value = context.GetValue(start, end);
            var postfix = "";
            if (context.IsIdentifier(out var position, end))
            {
                postfix = context.GetValue(end, position);
                end = position;
            }

            if (PostfixToConverter.TryGetValue(postfix, out var convert))
            {
                var result = convert(value, integer, postfix);
                if (result != null)
                    context.SetPosition(end);
                return result;
            }

            return null;
        }

        #endregion

        #region Methods

        public static IExpressionNode? Convert(string value, bool integer, string postfix)
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

        public delegate IExpressionNode? ConvertDelegate(string value, bool integer, string postfix);

        #endregion
    }
}