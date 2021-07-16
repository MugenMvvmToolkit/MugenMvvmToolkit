using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class UnaryTokenParserTest : TokenParserTestBase<UnaryTokenParser>
    {
        [Fact]
        public void TryParseShouldIgnoreNotUnaryExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseUnaryExpression()
        {
            var token = new UnaryTokenType("-");
            const string memberName = "test";
            Parser.Mapping.Clear();

            Context.Parsers = new ITokenParserComponent[] { new DigitTokenParser(), new MemberTokenParser() };

            Context.Initialize("-1", Metadata);
            Parser.TryParse(Context, null).ShouldBeNull();

            Parser.Mapping['-'] = new[] { token };
            Context.Initialize("-1", Metadata);
            Parser.TryParse(Context, null).ShouldEqual(new UnaryExpressionNode(token, ConstantExpressionNode.Get(1)));

            Context.Initialize($"-{memberName}.{memberName}", Metadata);
            Parser.TryParse(Context, null).ShouldEqual(new UnaryExpressionNode(token, new MemberExpressionNode(new MemberExpressionNode(null, memberName), memberName)));

            token.IsSingleExpression = true;
            Context.Initialize($"-{memberName}.{memberName}", Metadata);
            Parser.TryParse(Context, null).ShouldEqual(new UnaryExpressionNode(token, new MemberExpressionNode(null, memberName)));
        }
    }
}