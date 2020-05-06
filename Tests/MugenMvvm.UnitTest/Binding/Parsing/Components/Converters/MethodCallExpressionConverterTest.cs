using System.Linq.Expressions;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Converters
{
    public class MethodCallExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotMethodExpression()
        {
            var component = new MethodCallExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }


        [Fact]
        public void TryConvertShouldConvertResourceMethod()
        {
            var target = new TestResourceExtensionClass();
            var method = target.GetType().GetMethod(nameof(target.MethodResourceExt));
            var ctx = new ExpressionConverterContext<Expression>();
            var component = new MethodCallExpressionConverter();
            var expressionNode = component.TryConvert(ctx, Expression.Call(Expression.Constant(target), method, Expression.Constant("")));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.MethodResource)));
        }

        [Fact]
        public void TryConvertShouldConvertMethod()
        {
            var target = new TestConverterClass();
            var method = target.GetType().GetMethod(nameof(target.Method));
            var ctx = new ExpressionConverterContext<Expression>();
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get(target), method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);
            ctx.SetExpression(targetExp, expectedResult.Target!);

            var component = new MethodCallExpressionConverter();
            component.TryConvert(ctx, Expression.Call(targetExp, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertMethodStatic()
        {
            var method = typeof(TestConverterClass).GetMethod(nameof(TestConverterClass.MethodStatic));
            var ctx = new ExpressionConverterContext<Expression>();
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get<TestConverterClass>(), method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);

            var component = new MethodCallExpressionConverter();
            component.TryConvert(ctx, Expression.Call(null, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceMethod()
        {
            var target = new TestConverterClass();
            var method = target.GetType().GetMethod(nameof(target.Method));
            var ctx = new ExpressionConverterContext<Expression>();
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)), method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);
            ctx.SetExpression(targetExp, expectedResult.Target!);

            var component = new MethodCallExpressionConverter();
            component.TryConvert(ctx, Expression.Call(targetExp, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertMethodStaticExtension()
        {
            var method = typeof(TestConverterStaticClass).GetMethod(nameof(TestConverterStaticClass.TestMethod));
            var ctx = new ExpressionConverterContext<Expression>();
            var targetExp = Expression.Constant("");
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.EmptyString, method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);
            ctx.SetExpression(targetExp, ConstantExpressionNode.EmptyString);

            var component = new MethodCallExpressionConverter();
            component.TryConvert(ctx, Expression.Call(null, method, targetExp, argExp)).ShouldEqual(expectedResult);
        }

        #endregion
    }
}