using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class MemberExpressionConverterTest : ExpressionConverterTestBase<MemberExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertMember()
        {
            var target = new TestConverterClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.Property))!;
            var targetExp = Expression.Constant(target);

            var expectedResult = new MemberExpressionNode(ConstantExpressionNode.Get(target), propertyInfo.Name);
            Context.SetExpression(targetExp, expectedResult.Target!);
            Converter.TryConvert(Context, Expression.Property(targetExp, propertyInfo)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertResourceMember()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.PropertyResourceExt));
            var expressionNode = Converter.TryConvert(Context, Expression.Property(Expression.Constant(target), propertyInfo!));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.PropertyResource)));
        }

        [Fact]
        public void TryConvertShouldConvertStaticMember()
        {
            var propertyInfo = typeof(TestConverterClass).GetProperty(nameof(TestConverterClass.PropertyStatic))!;
            var expectedResult = new MemberExpressionNode(TypeAccessExpressionNode.Get<TestConverterClass>(), propertyInfo.Name);
            Converter.TryConvert(Context, Expression.Property(null, propertyInfo)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceMember()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperty(nameof(target.Property))!;
            var expressionNode = Converter.TryConvert(Context, Expression.Property(Expression.Constant(target), propertyInfo));

            var expectedResult = new MemberExpressionNode(
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)),
                propertyInfo.Name);
            expressionNode.ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotMemberExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}