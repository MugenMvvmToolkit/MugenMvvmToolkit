using System;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Constants;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class DigitTokenParserTest : TokenParserTestBase<DigitTokenParser>
    {
        public DigitTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new ConstantTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotDigitExpression()
        {
            Context.Initialize(InternalConstant.Null, DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseCustomDigitExpression()
        {
            var invokeCount = 0;
            Context.Initialize("1dp", DefaultMetadata);
            Parser.PostfixToConverter.Clear();
            Parser.PostfixToConverter["dp"] = (value, integer, postfix, context, format) =>
            {
                ++invokeCount;
                format.ShouldEqual(Parser.FormatProvider);
                value.ToString().ShouldEqual("1");
                integer.ShouldBeTrue();
                postfix.ShouldEqual("dp");
                context.ShouldEqual(Context);
                return ConstantExpressionNode.Get(1);
            };
            Parser.TryParse(Context, null).ShouldEqual(ConstantExpressionNode.Get(1));
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("1.1", 1.1)]
        [InlineData("1e-1", 1e-1)]
        [InlineData("1e+1", 1e+1)]
        [InlineData("1.1f", 1.1f)]
        [InlineData("1.1F", 1.1F)]
        [InlineData("1.1d", 1.1d)]
        [InlineData("1.1D", 1.1D)]
        [InlineData("1.1m", 1.1)]
        [InlineData("1.1M", 1.1)]
        [InlineData("1u", 1u)]
        [InlineData("1U", 1U)]
        [InlineData("1ul", 1ul)]
        [InlineData("1UL", 1UL)]
        [InlineData("1Ul", 1Ul)]
        [InlineData("1uL", 1uL)]
        [InlineData("1e-", null)]
        [InlineData("1e+", null)]
        [InlineData("1.1UL", null)]
        public void TryParseShouldParseDigitExpression(string expression, object result)
        {
            if (expression.EndsWith("m", StringComparison.OrdinalIgnoreCase))
                result = Convert.ToDecimal(result);

            Context.Initialize(expression, DefaultMetadata);
            if (result == null)
                Parser.TryParse(Context, null).ShouldBeNull();
            else
                Parser.TryParse(Context, null).ShouldEqual(ConstantExpressionNode.Get(result));
        }
    }
}