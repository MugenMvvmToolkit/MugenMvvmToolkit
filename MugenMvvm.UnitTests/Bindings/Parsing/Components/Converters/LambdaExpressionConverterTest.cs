using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Components.Converters
{
    public class LambdaExpressionConverterTest : ExpressionConverterTestBase<LambdaExpressionConverter>
    {
        [Fact]
        public void TryConvertShouldConvertLambdaExpression()
        {
            var p1 = Expression.Parameter(typeof(string), "1");
            var p2 = Expression.Parameter(typeof(int), "2");
            var body = Expression.Constant("");
            var expectedResult = new LambdaExpressionNode(ConstantExpressionNode.EmptyString, new[]
            {
                new ParameterExpressionNode(p1.Name!),
                new ParameterExpressionNode(p2.Name!)
            });

            Context.Converters = new IExpressionConverterComponent<Expression>[]
            {
                new TestExpressionConverterComponent<Expression>
                {
                    TryConvert = (context, expression) =>
                    {
                        context.TryGetExpression(p1).ShouldEqual(new ParameterExpressionNode(p1.Name!));
                        context.TryGetExpression(p2).ShouldEqual(new ParameterExpressionNode(p2.Name!));
                        return ConstantExpressionNode.EmptyString;
                    }
                }
            };

            Converter.TryConvert(Context, Expression.Lambda(body, p1, p2)).ShouldEqual(expectedResult);
            Context.TryGetExpression(p1).ShouldBeNull();
            Context.TryGetExpression(p2).ShouldBeNull();
        }

        [Fact]
        public void TryConvertShouldIgnoreNotLambdaExpression() => Converter.TryConvert(Context, Expression.Parameter(typeof(object))).ShouldBeNull();
    }
}