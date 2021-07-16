using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Metadata;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class BinaryTokenParserTest : TokenParserTestBase<BinaryTokenParser>
    {
        public BinaryTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotBinaryExpression()
        {
            Context.Initialize("1", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldIgnoreNotSupportBinaryExpression()
        {
            Parser.Tokens.Clear();
            Context.Initialize("+ 1", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseAlias()
        {
            const string alias = "aa";
            const string sign = "/";
            var tokenType = new BinaryTokenType(sign, 0, alias);
            Parser.Tokens.Clear();
            Parser.Tokens.Add(tokenType);

            Context.Initialize($"{alias} 2", Metadata);
            Parser.TryParse(Context, ConstantExpressionNode.Get(1)).ShouldEqual(new BinaryExpressionNode(tokenType, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)));
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryParseShouldParseBinaryExpression(StringTokenParserContext ctx, IExpressionNode? expression, IExpressionNode result) =>
            Parser.TryParse(ctx, expression).ShouldEqual(result);

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                GetBinary(BinaryTokenType.Multiplication),
                GetBinary(BinaryTokenType.Division),
                GetBinary(BinaryTokenType.Remainder),
                GetBinary(BinaryTokenType.Addition),
                GetBinary(BinaryTokenType.Subtraction),
                GetBinary(BinaryTokenType.LeftShift),
                GetBinary(BinaryTokenType.RightShift),
                GetBinary(BinaryTokenType.LessThan),
                GetBinary(BinaryTokenType.GreaterThan),
                GetBinary(BinaryTokenType.LessThanOrEqual),
                GetBinary(BinaryTokenType.GreaterThanOrEqual),
                GetBinary(BinaryTokenType.Equality),
                GetBinary(BinaryTokenType.NotEqual),
                GetBinary(BinaryTokenType.LogicalAnd),
                GetBinary(BinaryTokenType.LogicalXor),
                GetBinary(BinaryTokenType.LogicalOr),
                GetBinary(BinaryTokenType.ConditionalAnd),
                GetBinary(BinaryTokenType.ConditionalOr),
                GetBinary(BinaryTokenType.NullCoalescing)
            };

        private static object?[] GetBinary(BinaryTokenType binaryToken)
        {
            var context = new StringTokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            var result = new BinaryExpressionNode(binaryToken, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2));
            context.Initialize($"{binaryToken.Value} {((IConstantExpressionNode)result.Right).Value}", EmptyMetadataContext.Instance);
            return new object[]
            {
                context, result.Left, result
            };
        }
    }
}