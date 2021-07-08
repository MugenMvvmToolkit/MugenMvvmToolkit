using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class NewArrayExpressionConverterTest : ExpressionConverterTestBase<NewArrayExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertNewArrayExpression()
        {
            var arg1 = Expression.Constant(1);
            var arg2 = Expression.Constant(2);

            Context.SetExpression(arg1, ConstantExpressionNode.Get(1));
            Context.SetExpression(arg2, ConstantExpressionNode.Get(2));
            var expectedResult = new MethodCallExpressionNode(TypeAccessExpressionNode.Get<NewArrayExpressionConverter>(), nameof(NewArrayExpressionConverter.NewArrayInit), new[]
            {
                ConstantExpressionNode.Get(1),
                ConstantExpressionNode.Get(2)
            }, new[] { typeof(int).AssemblyQualifiedName! });

            Converter.TryConvert(Context, Expression.NewArrayInit(typeof(int), arg1, arg2)).ShouldEqual(expectedResult);
        }

        [Fact]
        public void TryConvertShouldIgnoreNotNewArrayExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}