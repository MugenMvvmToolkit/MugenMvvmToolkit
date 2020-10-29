using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class AssignmentTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotAssignExpression()
        {
            var component = new AssignmentTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }


        [Fact]
        public void TryParseShouldParseAssignExpression()
        {
            var component = new AssignmentTokenParser();
            var ctx = new TokenParserContext {Parsers = new ITokenParserComponent[] {new DigitTokenParser()}};

            ctx.Initialize("=1", DefaultMetadata);
            component.TryParse(ctx, MemberExpressionNode.Empty).ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, MemberExpressionNode.Empty, ConstantExpressionNode.Get(1)));
        }

        #endregion
    }
}