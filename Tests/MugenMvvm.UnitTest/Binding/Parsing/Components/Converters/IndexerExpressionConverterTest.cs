using System.Linq;
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
    public class IndexerExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotIndexExpression()
        {
            var component = new IndexerExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertResourceIndexer()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(int));
            var ctx = new ExpressionConverterContext<Expression>();
            var component = new IndexerExpressionConverter();
            var expressionNode = component.TryConvert(ctx, Expression.MakeIndex(Expression.Constant(target), propertyInfo, new[] {Expression.Constant(1)}));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.IndexerResource)));
        }

        [Fact]
        public void TryConvertShouldConvertIndexer()
        {
            var target = new TestConverterClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(object));
            var ctx = new ExpressionConverterContext<Expression>();
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new IndexExpressionNode(ConstantExpressionNode.Get(target), new[]
            {
                ConstantExpressionNode.Null
            });
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);
            ctx.SetExpression(targetExp, expectedResult.Target!);

            var component = new IndexerExpressionConverter();
            component.TryConvert(ctx, Expression.MakeIndex(targetExp, propertyInfo, new[] {argExp})).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceIndexer()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(object));
            var ctx = new ExpressionConverterContext<Expression>();
            var argExp = Expression.Constant(null);
            ctx.SetExpression(argExp, ConstantExpressionNode.Null);
            var component = new IndexerExpressionConverter();
            var expressionNode = component.TryConvert(ctx, Expression.MakeIndex(Expression.Constant(target), propertyInfo, new[] {argExp}));

            var expectedResult = new IndexExpressionNode(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)), new[]
            {
                ConstantExpressionNode.Null
            });
            expressionNode.ShouldEqual(expectedResult);
        }

        #endregion
    }
}