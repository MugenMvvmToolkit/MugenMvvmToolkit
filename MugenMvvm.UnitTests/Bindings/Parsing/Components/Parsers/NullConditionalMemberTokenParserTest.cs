using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class NullConditionalMemberTokenParserTest : TokenParserTestBase<NullConditionalMemberTokenParser>
    {
        [Fact]
        public void TryParseShouldIgnoreNotNullConditionalExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseNullConditionalExpression()
        {
            const string memberName = "Test";
            Context.Parsers = new ITokenParserComponent[] { new MemberTokenParser(), new IndexerTokenParser() };
            Context.Initialize($"?.{memberName}", Metadata);

            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            Context.Initialize($"?      .{memberName}", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null), memberName));

            Context.Initialize($"?[{memberName}]", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldEqual(new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null),
                new MemberExpressionNode(null, memberName)));

            Context.Initialize($"?      [{memberName}]", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldEqual(new IndexExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Null),
                new MemberExpressionNode(null, memberName)));

            Context.Initialize($"?{memberName}", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldBeNull();

            Context.Initialize("?.", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldBeNull();
        }
    }
}