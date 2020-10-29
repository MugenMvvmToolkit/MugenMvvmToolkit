using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class MemberExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotMemberExpression()
        {
            var component = new MemberExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertResourceMember()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.PropertyResourceExt));
            var ctx = new ExpressionConverterContext<Expression>();
            var component = new MemberExpressionConverter();
            var expressionNode = component.TryConvert(ctx, Expression.Property(Expression.Constant(target), propertyInfo));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.PropertyResource)));
        }

        [Fact]
        public void TryConvertShouldConvertMember()
        {
            var target = new TestConverterClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.Property))!;
            var ctx = new ExpressionConverterContext<Expression>();
            var targetExp = Expression.Constant(target);

            var expectedResult = new MemberExpressionNode(ConstantExpressionNode.Get(target), propertyInfo.Name);
            ctx.SetExpression(targetExp, expectedResult.Target!);

            var component = new MemberExpressionConverter();
            component.TryConvert(ctx, Expression.Property(targetExp, propertyInfo)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertStaticMember()
        {
            var propertyInfo = typeof(TestConverterClass).GetProperty(nameof(TestConverterClass.PropertyStatic))!;
            var ctx = new ExpressionConverterContext<Expression>();

            var expectedResult = new MemberExpressionNode(ConstantExpressionNode.Get<TestConverterClass>(), propertyInfo.Name);
            var component = new MemberExpressionConverter();
            component.TryConvert(ctx, Expression.Property(null, propertyInfo)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceMember()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.Property))!;
            var ctx = new ExpressionConverterContext<Expression>();
            var component = new MemberExpressionConverter();
            var expressionNode = component.TryConvert(ctx, Expression.Property(Expression.Constant(target), propertyInfo));

            var expectedResult = new MemberExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)), propertyInfo.Name);
            expressionNode.ShouldEqual(expectedResult);
        }

        #endregion
    }
}