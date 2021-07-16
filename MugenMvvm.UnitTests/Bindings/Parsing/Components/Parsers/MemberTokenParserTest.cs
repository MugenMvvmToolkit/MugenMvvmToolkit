using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class MemberTokenParserTest : TokenParserTestBase<MemberTokenParser>
    {
        public MemberTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotMemberExpression()
        {
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseMemberExpression()
        {
            const string memberName = "Test";

            Context.Initialize(memberName, Metadata);
            Parser.TryParse(Context, null).ShouldEqual(new MemberExpressionNode(null, memberName));

            Context.Initialize($".{memberName}", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldEqual(new MemberExpressionNode(ConstantExpressionNode.Null, memberName));

            Context.Initialize($"{memberName}", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldBeNull();
        }
    }
}