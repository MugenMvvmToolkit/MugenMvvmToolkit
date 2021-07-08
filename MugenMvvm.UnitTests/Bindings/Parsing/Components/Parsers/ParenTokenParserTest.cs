using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class ParenTokenParserTest : TokenParserTestBase<ParenTokenParser>
    {
        [Fact]
        public void TryParseShouldIgnoreNotParenExpression()
        {
            Context.Parsers = new DigitTokenParser();
            Context.Initialize("1", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseNullConditionalExpression()
        {
            var expected = new BinaryExpressionNode(BinaryTokenType.Addition, ConstantExpressionNode.Get(1),
                new BinaryExpressionNode(BinaryTokenType.Addition,
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(1)),
                    new BinaryExpressionNode(BinaryTokenType.Subtraction, ConstantExpressionNode.Get(2), ConstantExpressionNode.Get(2))));

            Context.Parsers = new ITokenParserComponent[] { new BinaryTokenParser(), new DigitTokenParser(), Parser };

            Context.Initialize("(1+(1-1+(2-2)))", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(expected);

            Context.Initialize("(1+(1-1+(2-2))", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }
    }
}