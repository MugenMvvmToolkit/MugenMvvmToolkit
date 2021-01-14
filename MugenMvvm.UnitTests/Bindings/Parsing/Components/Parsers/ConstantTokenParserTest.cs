using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ConstantTokenParserTest : UnitTestBase
    {
        [Fact]
        public void TryParseShouldIgnoreNotConstantExpression()
        {
            var component = new ConstantTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseConstantExpression()
        {
            const string name = "test";
            var component = new ConstantTokenParser();
            var ctx = new TokenParserContext();
            ctx.Initialize(name, DefaultMetadata);
            component.LiteralToExpression.Clear();
            component.LiteralToExpression[name] = ConstantExpressionNode.Null;

            component.TryParse(ctx, null).ShouldEqual(ConstantExpressionNode.Null);
            component.LiteralToExpression.Clear();
            component.TryParse(ctx, null).ShouldBeNull();
        }
    }
}