using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class IndexerTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotIndexerExpression()
        {
            var component = new IndexerTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new ConstantTokenParser()}
            };
            ctx.Initialize("null", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseIndexerExpression()
        {
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("[1,2 , 3]", DefaultMetadata);

            var component = new IndexerTokenParser();
            component.TryParse(ctx, null).ShouldEqual(new IndexExpressionNode(null, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3)}));

            ctx.Initialize("[1,2 , 3]", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null)
                .ShouldEqual(new IndexExpressionNode(ConstantExpressionNode.Null, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3)}));

            ctx.Initialize("[1,2,]", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        #endregion
    }
}