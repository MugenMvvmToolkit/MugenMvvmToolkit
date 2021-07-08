using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class StringTokenParserTest : TokenParserTestBase<StringTokenParser>
    {
        [Fact]
        public void TryParseShouldIgnoreNotStringExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("1", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
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
            Context.Initialize(expression, DefaultMetadata);
            if (result == null)
                Parser.TryParse(Context, null).ShouldBeNull();
            else
                Parser.TryParse(Context, null).ShouldEqual(ConstantExpressionNode.Get(result));
        }

        [Fact]
        public void TryParseShouldParseStringFormatExpression()
        {
            Context.Parsers = new ITokenParserComponent[] { new DigitTokenParser(), new BinaryTokenParser() };

            var expected = new MethodCallExpressionNode(TypeAccessExpressionNode.Get<string>(), nameof(string.Format),
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0} test {1}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            Context.Initialize("$@'{1+1} test {2}'", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(expected);

            expected = new MethodCallExpressionNode(TypeAccessExpressionNode.Get<string>(), nameof(string.Format),
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0:n} test {1:t}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            Context.Initialize("@$'{1+1:n} test {2:t}'", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(expected);

            expected = new MethodCallExpressionNode(TypeAccessExpressionNode.Get<string>(), nameof(string.Format),
                new IExpressionNode[]
                {
                    ConstantExpressionNode.Get("{0:n} {test} {1:t}"),
                    new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)), ConstantExpressionNode.Get(2)
                }, new string[0]);
            Context.Initialize("$'{1+1:n} {{test}} {2:t}'", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(expected);
        }
    }
}