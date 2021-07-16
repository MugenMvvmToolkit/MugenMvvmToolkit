using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ConditionTokenParserTest : TokenParserTestBase<ConditionTokenParser>
    {
        public ConditionTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotConditionExpression()
        {
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseConditionExpression()
        {
            Context.Initialize("? 2 : 3", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1))
                  .ShouldEqual(new ConditionExpressionNode(ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3)));
        }
    }
}