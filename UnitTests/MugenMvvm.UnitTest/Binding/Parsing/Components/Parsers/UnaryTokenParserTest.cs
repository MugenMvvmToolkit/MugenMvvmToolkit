using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class UnaryTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotUnaryExpression()
        {
            var component = new UnaryTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseUnaryExpression()
        {
            var token = new UnaryTokenType("-");
            const string memberName = "test";
            var component = new UnaryTokenParser();
            component.Mapping.Clear();

            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[] {new DigitTokenParser(), new MemberTokenParser()}
            };

            ctx.Initialize("-1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();

            component.Mapping['-'] = new[] {token};
            ctx.Initialize("-1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new UnaryExpressionNode(token, ConstantExpressionNode.Get(1)));

            ctx.Initialize($"-{memberName}.{memberName}", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new UnaryExpressionNode(token, new MemberExpressionNode(new MemberExpressionNode(null, memberName), memberName)));

            token.IsSingleExpression = true;
            ctx.Initialize($"-{memberName}.{memberName}", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new UnaryExpressionNode(token, new MemberExpressionNode(null, memberName)));
        }

        #endregion
    }
}