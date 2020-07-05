using System;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class DigitTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotDigitExpression()
        {
            var component = new DigitTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] { new ConstantTokenParser() }
            };
            ctx.Initialize("null", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
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
                result = System.Convert.ToDecimal(result);
            var component = new DigitTokenParser();
            var ctx = new TokenParserContext();
            ctx.Initialize(expression, DefaultMetadata);
            if (result == null)
                component.TryParse(ctx, null).ShouldBeNull();
            else
                component.TryParse(ctx, null).ShouldEqual(ConstantExpressionNode.Get(result));
        }

        [Fact]
        public void TryParseShouldParseCustomDigitExpression()
        {
            var invokeCount = 0;
            var component = new DigitTokenParser();
            var ctx = new TokenParserContext();
            ctx.Initialize("1dp", DefaultMetadata);
            component.PostfixToConverter.Clear();
            component.PostfixToConverter["dp"] = (value, integer, postfix, context, format) =>
            {
                ++invokeCount;
                format.ShouldEqual(component.FormatProvider);
                value.ToString().ShouldEqual("1");
                integer.ShouldBeTrue();
                postfix.ShouldEqual("dp");
                context.ShouldEqual(ctx);
                return ConstantExpressionNode.Get(1);
            };
            component.TryParse(ctx, null).ShouldEqual(ConstantExpressionNode.Get(1));
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}