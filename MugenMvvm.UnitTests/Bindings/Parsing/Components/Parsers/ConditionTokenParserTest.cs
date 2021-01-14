using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ConditionTokenParserTest : UnitTestBase
    {
        [Fact]
        public void TryParseShouldIgnoreNotConditionExpression()
        {
            var component = new ConditionTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseConditionExpression()
        {
            var component = new ConditionTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize("? 2 : 3", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Get(1))
                     .ShouldEqual(new ConditionExpressionNode(ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3)));
        }
    }
}