using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
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
            var ctx = new TokenParserContext {Parsers = new ITokenParserComponent[] {new MemberTokenParser(), new IndexerTokenParser()}};
            ctx.Initialize($"?.{memberName}", DefaultMetadata);

            var component = new NullConditionalMemberTokenParser();
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            ctx.Initialize($"?      .{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            ctx.Initialize($"?[{memberName}]", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), new[]
            {
                new MemberExpressionNode(null, memberName)
            }));

            ctx.Initialize($"?      [{memberName}]", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), new[]
            {
                new MemberExpressionNode(null, memberName)
            }));

            ctx.Initialize($"?{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();

            ctx.Initialize("?.", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();
        }

        #endregion
    }
}