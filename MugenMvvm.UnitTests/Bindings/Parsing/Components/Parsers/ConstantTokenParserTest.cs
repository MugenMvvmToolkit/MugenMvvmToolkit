using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ConstantTokenParserTest : TokenParserTestBase<ConstantTokenParser>
    {
        public ConstantTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotConstantExpression()
        {
            Context.Initialize("1", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseConstantExpression()
        {
            const string name = "test";
            Context.Initialize(name, DefaultMetadata);
            Parser.LiteralToExpression.Clear();
            Parser.LiteralToExpression[name] = ConstantExpressionNode.Null;

            Parser.TryParse(Context, null).ShouldEqual(ConstantExpressionNode.Null);
            Parser.LiteralToExpression.Clear();
            Parser.TryParse(Context, null).ShouldBeNull();
        }
    }
}