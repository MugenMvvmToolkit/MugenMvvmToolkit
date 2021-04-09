using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Constants;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class LambdaTokenParserTest : TokenParserTestBase<LambdaTokenParser>
    {
        [Fact]
        public void TryParseShouldIgnoreNotLambdaExpression()
        {
            Context.Parsers = new ConstantTokenParser();
            Context.Initialize(InternalConstant.Null, DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseLambdaExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("() => 1", DefaultMetadata);

            Parser.TryParse(Context, null).ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), default));

            Context.Initialize("(p1) => 1", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), new[] {new ParameterExpressionNode("p1")}));

            Context.Initialize("(p1, p2) => 1", DefaultMetadata);
            Parser.TryParse(Context, null)
                  .ShouldEqual(new LambdaExpressionNode(ConstantExpressionNode.Get(1), new[] {new ParameterExpressionNode("p1"), new ParameterExpressionNode("p2")}));
        }
    }
}