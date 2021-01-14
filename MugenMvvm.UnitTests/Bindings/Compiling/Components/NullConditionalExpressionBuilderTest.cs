using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class NullConditionalExpressionBuilderTest : UnitTestBase
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("t", 1)]
        public void TryBuildShouldBuildNullConditionalExpression1(string? value, int? result)
        {
            var node = new MemberExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(value, typeof(string))), nameof(string.Length));
            var component = new NullConditionalExpressionBuilder();
            TestExpressionBuilderContext ctx = null!;
            ctx = new TestExpressionBuilderContext
            {
                Build = expressionNode =>
                {
                    if (expressionNode is IConstantExpressionNode constant)
                        return Expression.Constant(constant.Value, constant.Type);
                    if (expressionNode is IMemberExpressionNode memberExpression)
                    {
                        var target = ((IExpressionBuilderContext) ctx).Build(memberExpression.Target!);
                        return Expression.MakeMemberAccess(target, typeof(string).GetProperty(memberExpression.Member)!);
                    }

                    return null;
                }
            };
            var expression = component.TryBuild(ctx, node)!;
            expression.Invoke().ShouldEqual(result);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(1, "1")]
        public void TryBuildShouldBuildNullConditionalExpression2(int? value, string? result)
        {
            var node = new MethodCallExpressionNode(new NullConditionalMemberExpressionNode(ConstantExpressionNode.Get(value, value.HasValue ? typeof(int) : typeof(int?))),
                nameof(ToString),
                Default.Array<IExpressionNode>());
            var component = new NullConditionalExpressionBuilder();
            TestExpressionBuilderContext ctx = null!;
            ctx = new TestExpressionBuilderContext
            {
                Build = expressionNode =>
                {
                    if (expressionNode is IConstantExpressionNode constant)
                        return Expression.Constant(constant.Value, constant.Type);
                    if (expressionNode is IMethodCallExpressionNode methodCall)
                    {
                        var target = ((IExpressionBuilderContext) ctx).Build(methodCall.Target!);
                        return Expression.Call(target.ConvertIfNeed(typeof(object), false), typeof(object).GetMethods().FirstOrDefault(info => info.Name == nameof(ToString))!);
                    }

                    return null;
                }
            };
            var expression = component.TryBuild(ctx, node)!;
            expression.Invoke().ShouldEqual(result);
        }

        [Fact]
        public void TryBuildShouldIgnoreNotNullConditionalExpression()
        {
            var component = new NullConditionalExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }
    }
}