using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class MethodCallExpressionConverterTest : ExpressionConverterTestBase<MethodCallExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertMethod()
        {
            var target = new TestConverterClass();
            var method = target.GetType().GetMethod(nameof(target.Method))!;
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get(target), method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            Context.SetExpression(targetExp, expectedResult.Target!);
            Converter.TryConvert(Context, Expression.Call(targetExp, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertMethodStatic()
        {
            var method = typeof(TestConverterClass).GetMethod(nameof(TestConverterClass.MethodStatic))!;
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get<TestConverterClass>(), method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            Converter.TryConvert(Context, Expression.Call(null, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertMethodStaticExtension()
        {
            var method = typeof(TestConverterStaticClass).GetMethod(nameof(TestConverterStaticClass.TestMethod))!;
            var targetExp = Expression.Constant("");
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.EmptyString, method.Name, new[]
            {
                ConstantExpressionNode.Null
            });
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            Context.SetExpression(targetExp, ConstantExpressionNode.EmptyString);
            Converter.TryConvert(Context, Expression.Call(null, method, targetExp, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertResourceMethod()
        {
            var target = new TestResourceExtensionClass();
            var method = target.GetType().GetMethod(nameof(target.MethodResourceExt));
            var expressionNode = Converter.TryConvert(Context, Expression.Call(Expression.Constant(target), method!, Expression.Constant("")));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.MethodResource)));
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceMethod()
        {
            var target = new TestConverterClass();
            var method = target.GetType().GetMethod(nameof(target.Method))!;
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new MethodCallExpressionNode(
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)), method.Name, new[]
                {
                    ConstantExpressionNode.Null
                });
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            Context.SetExpression(targetExp, expectedResult.Target!);
            Converter.TryConvert(Context, Expression.Call(targetExp, method, argExp)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotMethodExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}