using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Components.Parsers
{
    public class MemberTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotMemberExpression()
        {
            var component = new MemberTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseMemberExpression()
        {
            const string memberName = "Test";
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize(memberName, DefaultMetadata);

            var component = new MemberTokenParser();
            component.TryParse(ctx, null).ShouldEqual(new MemberExpressionNode(null, memberName));

            ctx.Initialize($".{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(ConstantExpressionNode.Null, memberName));

            ctx.Initialize($"{memberName}", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();
        }

        #endregion
    }
}