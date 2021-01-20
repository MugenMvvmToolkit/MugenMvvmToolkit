using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class MethodCallTokenParserTest : UnitTestBase
    {
        [Fact]
        public void TryParseShouldIgnoreNotMethodCallExpression()
        {
            var component = new MethodCallTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };
            ctx.Initialize("1", DefaultMetadata);
            component.TryParse(ctx, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseMethodCallExpression()
        {
            const string memberName = "Test";
            const string typeArg1 = "t1";
            const string typeArg2 = "t2";
            var component = new MethodCallTokenParser();
            var ctx = new TokenParserContext
            {
                Parsers = new[] {new DigitTokenParser()}
            };

            ctx.Initialize($"{memberName}()", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, default));

            ctx.Initialize($"{memberName}<{typeArg1}, {typeArg2}>()", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, default, new[] {typeArg1, typeArg2}));

            ctx.Initialize($"{memberName}(1,2)", DefaultMetadata);
            component.TryParse(ctx, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)}));

            ctx.Initialize($"{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            component.TryParse(ctx, null)
                     .ShouldEqual(new MethodCallExpressionNode(null, memberName, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)}, new[] {typeArg1, typeArg2}));

            ctx.Initialize($".{memberName}()", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null)
                     .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, default));

            ctx.Initialize($".{memberName}<{typeArg1}, {typeArg2}>()", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null)
                     .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, default, new[] {typeArg1, typeArg2}));

            ctx.Initialize($".{memberName}(1,2)", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null)
                     .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)}));

            ctx.Initialize($".{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null)
                     .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, new[] {ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2)},
                         new[] {typeArg1, typeArg2}));

            ctx.Initialize($"{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            component.TryParse(ctx, ConstantExpressionNode.Null).ShouldBeNull();
        }
    }
}