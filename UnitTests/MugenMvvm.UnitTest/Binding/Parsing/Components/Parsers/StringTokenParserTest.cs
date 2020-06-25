using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class StringTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotStringExpression()
        {
            var component = new StringTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Theory]
        [InlineData("'test'", "test")]
        [InlineData("'t'", "t")]
        [InlineData("\"t\"", "t")]
        [InlineData("\"test\"", "test")]
        [InlineData("&amp;test&amp;", "test")]
        [InlineData("'\\0\\n'", "\0\n")]
        [InlineData("@'\"\"t\"\"'", "\"t\"")]
        [InlineData("'t", null)]
        [InlineData("\"t", null)]
        [InlineData("t\"", null)]
        [InlineData("@t\"\"", null)]
        public void TryParseShouldParseStringExpression(string expression, object result)
        {
            var component = new StringTokenParser();
            var ctx = new TokenParserContext();
            ctx.Initialize(expression, DefaultMetadata);
            if (result == null)
                component.TryParse(ctx, null).ShouldBeNull();
            else
                component.TryParse(ctx, null).ShouldEqual(ConstantExpressionNode.Get(result));
        }

        [Fact]
        public void TryParseShouldParseStringFormatExpression()
        {
            var component = new StringTokenParser();
            var ctx = new TokenParserContext {Parsers = new ITokenParserComponent[] {new DigitTokenParser(), new BinaryTokenParser()}};

            var expected = new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string)), "Format",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0} test {1}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            ctx.Initialize("$@'{1+1} test {2}'", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(expected);

            expected = new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string)), "Format",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0:n} test {1:t}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            ctx.Initialize("@$'{1+1:n} test {2:t}'", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(expected);

            expected = new MethodCallExpressionNode(ConstantExpressionNode.Get(typeof(string)), "Format",
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0:n} {test} {1:t}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            ctx.Initialize("$'{1+1:n} {{test}} {2:t}'", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(expected);
        }

        #endregion
    }
}