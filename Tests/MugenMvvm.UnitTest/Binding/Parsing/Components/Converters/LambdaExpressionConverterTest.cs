using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Parsing.Components.Converters
{
    public class LambdaExpressionConverterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryConvertShouldIgnoreNotLambdaExpression()
        {
            var component = new LambdaExpressionConverter();
            var ctx = new ExpressionConverterContext<Expression>();
            component.TryConvert(ctx, Expression.Parameter(typeof(object))).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldConvertLambdaExpression()
        {
            var p1 = Expression.Parameter(typeof(string), "1");
            var p2 = Expression.Parameter(typeof(int), "2");
            var body = Expression.Constant("");
            var expectedResult = new LambdaExpressionNode(ConstantExpressionNode.EmptyString, new[]
            {
                new ParameterExpressionNode(p1.Name),
                new ParameterExpressionNode(p2.Name)
            });
            var ctx = new ExpressionConverterContext<Expression>
            {
                Converters = new IExpressionConverterComponent<Expression>[]
                {
                    new TestExpressionConverterComponent<Expression>
                    {
                        TryConvert = (context, expression) =>
                        {
                            context.TryGetExpression(p1).ShouldEqual(new ParameterExpressionNode(p1.Name));
                            context.TryGetExpression(p2).ShouldEqual(new ParameterExpressionNode(p2.Name));
                            return ConstantExpressionNode.EmptyString;
                        }
                    }
                }
            };

            var component = new LambdaExpressionConverter();
            component.TryConvert(ctx, Expression.Lambda(body, p1, p2)).ShouldEqual(expectedResult);
            ctx.TryGetExpression(p1).ShouldBeNull();
            ctx.TryGetExpression(p2).ShouldBeNull();
        }

        #endregion
    }
}