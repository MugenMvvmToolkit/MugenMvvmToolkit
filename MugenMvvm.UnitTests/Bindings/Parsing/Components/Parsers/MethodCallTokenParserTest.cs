using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Parsers
{
    public class MethodCallTokenParserTest : TokenParserTestBase<MethodCallTokenParser>
    {
        public MethodCallTokenParserTest(ITestOutputHelper? outputHelper = null) : base(new DigitTokenParser(), outputHelper)
        {
        }

        [Fact]
        public void TryParseShouldIgnoreNotMethodCallExpression()
        {
            Context.Initialize("1", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldBeNull();
        }

        [Fact]
        public void TryParseShouldParseMethodCallExpression()
        {
            const string memberName = "Test";
            const string typeArg1 = "t1";
            const string typeArg2 = "t2";

            Context.Initialize($"{memberName}()", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, default));

            Context.Initialize($"{memberName}<{typeArg1}, {typeArg2}>()", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, default, new[] { typeArg1, typeArg2 }));

            Context.Initialize($"{memberName}(1,2)", DefaultMetadata);
            Parser.TryParse(Context, null).ShouldEqual(new MethodCallExpressionNode(null, memberName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) }));

            Context.Initialize($"{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            Parser.TryParse(Context, null)
                  .ShouldEqual(new MethodCallExpressionNode(null, memberName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) },
                      new[] { typeArg1, typeArg2 }));

            Context.Initialize($".{memberName}()", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, default));

            Context.Initialize($".{memberName}<{typeArg1}, {typeArg2}>()", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, default, new[] { typeArg1, typeArg2 }));

            Context.Initialize($".{memberName}(1,2)", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) }));

            Context.Initialize($".{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null)
                  .ShouldEqual(new MethodCallExpressionNode(ConstantExpressionNode.Null, memberName, new[] { ConstantExpressionNode.Get(1), ConstantExpressionNode.Get(2) },
                      new[] { typeArg1, typeArg2 }));

            Context.Initialize($"{memberName}<{typeArg1}, {typeArg2}>(1,2)", DefaultMetadata);
            Parser.TryParse(Context, ConstantExpressionNode.Null).ShouldBeNull();
        }
    }
}