using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ParenTokenParserTest : UnitTestBase
    {
        [Fact]
        public void TryParseShouldIgnoreNotParenExpression()
        {
            var component = new ParenTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseNullConditionalExpression()
        {
            var component = new ParenTokenParser();
            var expected = new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1),
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(2))));

            var ctx = new TokenParserContext {Parsers = new ITokenParserComponent[] {new BinaryTokenParser(), new DigitTokenParser(), component}};

            ctx.Initialize("(1+(1-1+(2-2)))", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(expected);

            ctx.Initialize("(1+(1-1+(2-2))", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }
    }
}