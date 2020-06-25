using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Parsers
{
    public class BinaryTokenParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseShouldIgnoreNotBinaryExpression()
        {
            var component = new BinaryTokenParser();
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
        public void TryParseShouldIgnoreNotSupportBinaryExpression()
        {
            var component = new BinaryTokenParser();
            component.Tokens.Clear();
            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize("+ 1", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Get(1)).ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryParseShouldParseBinaryExpression(TokenParserContext ctx, IExpressionNode? expression, IExpressionNode result)
        {
            new BinaryTokenParser().TryParse(ctx, expression).ShouldEqual(result);
        }

        [Fact]
        public void TryParseShouldParseAlias()
        {
            const string alias = "aa";
            const string sign = "/";
            var tokenType = new BinaryTokenType(sign, 0, alias);
            var component = new BinaryTokenParser();
            component.Tokens.Clear();
            component.Tokens.Add(tokenType);

            var ctx = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            ctx.Initialize($"{alias} 2", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Get(1)).ShouldEqual(new BinaryExpressionNode(tokenType, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)));
        }

        public static IEnumerable<object?[]> GetData()
        {
            return new[]
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
        }

        private static object?[] GetBinary(BinaryTokenType binaryToken)
        {
            var context = new TokenParserContext
            {
                Parsers = new ITokenParserComponent[]
                {
                    new DigitTokenParser()
                }
            };
            var result = new BinaryExpressionNode(binaryToken, ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2));
            context.Initialize($"{binaryToken.Value} {((IConstantExpressionNode) result.Right).Value}", DefaultMetadata);
            return new object[]
            {
                context, result.Left, result
            };
        }

        #endregion
    }
}