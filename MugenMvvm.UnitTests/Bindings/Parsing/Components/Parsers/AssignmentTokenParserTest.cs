using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class AssignmentTokenParserTest : TokenParserTestBase<AssignmentTokenParser>
    {
        public AssignmentTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotAssignExpression()
        {
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseAssignExpression()
        {
            Context.Initialize("=1", Metadata);
            Parser.TryParse(Context, MemberExpressionNode.Empty)
                  .ShouldEqual(new BinaryExpressionNode(BinaryTokenType.Assignment, MemberExpressionNode.Empty, ConstantExpressionNode.Get(1)));
        }
    }
}