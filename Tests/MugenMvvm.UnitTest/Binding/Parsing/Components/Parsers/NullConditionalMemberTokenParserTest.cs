using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class NullConditionalMemberTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotNullConditionalExpression()
        {
            var component = new NullConditionalMemberTokenParser();
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
            const string memberName = "Test";
            var ctx = new TokenParserContext {Parsers = new[] {new MemberTokenParser()}};
            ctx.Initialize($"?.{memberName}", DefaultMetadata);

            var component = new NullConditionalMemberTokenParser();
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            ctx.Initialize($"?      .{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            ctx.Initialize($"?{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();

            ctx.Initialize("?.", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();
        }

        #endregion
    }
}