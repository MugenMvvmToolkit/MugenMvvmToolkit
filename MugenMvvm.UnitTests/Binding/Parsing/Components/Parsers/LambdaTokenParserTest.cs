using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class LambdaTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotLambdaExpression()
        {
            var component = new LambdaTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new ConstantTokenParser()}
            };
            ctx.Initialize("null", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseLambdaExpression()
        {
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("() => 1", DefaultMetadata);

            var component = new LambdaTokenParser();
            component.TryParse(ctx, null).ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), null));

            ctx.Initialize("(p1) => 1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), new[] {new ParameterExpressionNode("p1")}));

            ctx.Initialize("(p1, p2) => 1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), new[] {new ParameterExpressionNode("p1"), new ParameterExpressionNode("p2")}));
        }

        #endregion
    }
}