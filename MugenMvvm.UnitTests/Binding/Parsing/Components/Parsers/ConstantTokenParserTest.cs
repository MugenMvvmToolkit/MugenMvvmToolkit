using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Components.Parsers
{
    public class ConstantTokenParserTest : UnitTestBase
    {
        #region Methods

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

        #endregion
    }
}