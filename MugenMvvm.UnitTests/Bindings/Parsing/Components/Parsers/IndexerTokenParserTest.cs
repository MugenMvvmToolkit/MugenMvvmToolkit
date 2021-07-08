using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Constants;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class IndexerTokenParserTest : TokenParserTestBase<IndexerTokenParser>
    {
        public IndexerTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new ConstantTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotIndexerExpression()
        {
            Context.Initialize(InternalConstant.Null, DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseIndexerExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("[1,2 , 3]", DefaultMetadata);

            Parser.TryParse(Context, null)
                  .ShouldEqual(new IndexExpressionNode(null, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3) }));

            Context.Initialize("[1,2 , 3]", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new IndexExpressionNode(ConstantExpressionNode.Null,
                      new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(3) }));

            Context.Initialize("[1,2,]", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }
    }
}