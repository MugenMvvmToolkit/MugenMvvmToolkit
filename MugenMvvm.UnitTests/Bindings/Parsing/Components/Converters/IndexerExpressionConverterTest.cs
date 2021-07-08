using System.Linq;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class IndexerExpressionConverterTest : ExpressionConverterTestBase<IndexerExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertIndexer()
        {
            var target = new TestConverterClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(object));
            var targetExp = Expression.Constant(target);
            var argExp = Expression.Constant(null);
            var expectedResult = new IndexExpressionNode(ConstantExpressionNode.Get(target), new[]
            {
                ConstantExpressionNode.Null
            });
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            Context.SetExpression(targetExp, expectedResult.Target!);

            Converter.TryConvert(Context, Expression.MakeIndex(targetExp, propertyInfo, new[] { argExp })).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldConvertResourceIndexer()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(int));
            var expressionNode = Converter.TryConvert(Context, Expression.MakeIndex(Expression.Constant(target), propertyInfo, new[] { Expression.Constant(1) }));
            expressionNode.ShouldEqual(new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.IndexerResource)));
        }

        [Fact]
        public void TryConvertShouldConvertTargetResourceIndexer()
        {
            var target = new TestResourceExtensionClass();
            var propertyInfo = target.GetType().GetProperties().Single(info => info.GetIndexParameters().FirstOrDefault()?.ParameterType == typeof(object));
            var argExp = Expression.Constant(null);
            Context.SetExpression(argExp, ConstantExpressionNode.Null);
            var expressionNode = Converter.TryConvert(Context, Expression.MakeIndex(Expression.Constant(target), propertyInfo, new[] { argExp }));

            var expectedResult = new IndexExpressionNode(
                new UnaryExpressionNode(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, TestResourceExtensionClass.ClassResource)), new[]
                {
                    ConstantExpressionNode.Null
                });
            expressionNode.ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotIndexExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}