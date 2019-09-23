using System.Collections.Generic;
using System.Globalization;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class DigitExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly Dictionary<string, ConvertDelegate> _postfixToConverters;

        #endregion

        #region Constructors

        public DigitExpressionParserComponent(Dictionary<string, ConvertDelegate>? postfixToConverters = null)
        {
            if (postfixToConverters == null)
            {
                ConvertDelegate defaultConverter = Convert;
                _postfixToConverters = new Dictionary<string, ConvertDelegate>
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
                _postfixToConverters = postfixToConverters;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Constant;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
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

            if (_postfixToConverters.TryGetValue(postfix, out var convert))
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
                        return new ConstantExpressionNode(ui, typeof(uint));
                    return null;
                case "ul":
                case "UL":
                case "Ul":
                case "uL":
                    if (ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ul))
                        return new ConstantExpressionNode(ul, typeof(ulong));
                    return null;
                case "":
                    if (integer)
                    {
                        if (ulong.TryParse(value, out var result))
                        {
                            if (result <= int.MaxValue)
                                return new ConstantExpressionNode((int)result, typeof(int));
                            if (result <= long.MaxValue)
                                return new ConstantExpressionNode((long)result, typeof(long));
                            return new ConstantExpressionNode(result, typeof(ulong));
                        }

                        return null;
                    }

                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
                        return new ConstantExpressionNode(r, typeof(ulong));
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