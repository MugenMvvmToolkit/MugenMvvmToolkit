using System.Linq.Expressions;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class NewArrayExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotNewArrayExpression()
        {
            var component = new NewArrayExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertNewArrayExpression()
        {
            var arg1 = Expression.Constant(1);
            var arg2 = Expression.Constant(2);
            var component = new NewArrayExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            ctx.SetExpression(arg1, ConstantExpressionNode.Get(1));
            ctx.SetExpression(arg2, ConstantExpressionNode.Get(2));
            var expectedResult = new MethodCallExpressionNode(ConstantExpressionNode.Get<NewArrayExpressionConverter>(), nameof(NewArrayExpressionConverter.NewArrayInit), new[]
            {
                ConstantExpressionNode.Get(1),
                ConstantExpressionNode.Get(2)
            }, new[] {typeof(int).AssemblyQualifiedName!});

            component.TryConvert(ctx, Expression.NewArrayInit(typeof(int), arg1, arg2)).ShouldEqual(expectedResult);
        }

        #endregion
    }
}