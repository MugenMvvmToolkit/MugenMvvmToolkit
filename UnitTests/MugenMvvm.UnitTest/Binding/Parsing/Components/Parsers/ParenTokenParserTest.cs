using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class ParenTokenParserTest : UnitTestBase
    {
        #region Methods

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
                new BinaryExpressionNode(BinaryTokenType.Addition, new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(2))));

            var ctx = new TokenParserContext {Parsers = new ITokenParserComponent[] {new BinaryTokenParser(), new DigitTokenParser(), component}};

            ctx.Initialize("(1+(1-1+(2-2)))", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(expected);

            ctx.Initialize("(1+(1-1+(2-2))", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        #endregion
    }
}