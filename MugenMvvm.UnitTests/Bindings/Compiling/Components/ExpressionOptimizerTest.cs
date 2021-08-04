using System.Linq.Expressions;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Compiling;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ExpressionOptimizerTest : UnitTestBase
    {
        public ExpressionOptimizerTest()
        {
            ExpressionCompiler.AddComponent(new ExpressionOptimizer());
        }

        [Fact]
        public void ShouldHandleNestedExpression()
        {
            var isNested = false;
            Expression expression = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(1));
            using var t = ExpressionCompiler.AddComponent(new TestExpressionBuilderComponent
            {
                TryBuild = (_, _) =>
                {
                    if (!isNested)
                    {
                        isNested = true;
                        ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)
                                          .ShouldEqual(expression);
                    }

                    return expression;
                }
            });
            var exp = ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression)exp).Value.ShouldEqual(2);
        }

        [Fact]
        public void ShouldOptimizeExpressions()
        {
            Expression expression = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(1));
            using var t = ExpressionCompiler.AddComponent(new TestExpressionBuilderComponent
            {
                TryBuild = (_, _) => expression
            });
            var exp = ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression)exp).Value.ShouldEqual(2);

            expression = Expression.MakeBinary(ExpressionType.Add, Expression.Convert(Expression.Constant(1, typeof(object)), typeof(int)), Expression.Constant(1));
            exp = ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression)exp).Value.ShouldEqual(2);

            expression = Expression.NewArrayInit(typeof(int), Expression.Constant(1), Expression.Constant(2));
            exp = ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((int[]?)((ConstantExpression)exp).Value).ShouldEqual(new[] { 1, 2 });

            expression = Expression.MakeBinary(ExpressionType.Add, Expression.Parameter(typeof(int), "test"), Expression.Constant(1));
            exp = ExpressionCompiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            exp.ShouldEqual(expression);
        }

        protected override IExpressionCompiler GetExpressionCompiler() => new ExpressionCompiler(ComponentCollectionManager);
    }
}