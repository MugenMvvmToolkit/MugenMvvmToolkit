using System.Linq.Expressions;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ExpressionOptimizerTest : UnitTestBase
    {
        private readonly ExpressionCompiler _compiler;

        public ExpressionOptimizerTest()
        {
            _compiler = new ExpressionCompiler();
            _compiler.AddComponent(new ExpressionOptimizer());
        }

        [Fact]
        public void ShouldHandleNestedExpression()
        {
            var isNested = false;
            Expression expression = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(1));
            using var t = _compiler.AddComponent(new TestExpressionBuilderComponent
            {
                TryBuild = (_, _) =>
                {
                    if (!isNested)
                    {
                        isNested = true;
                        _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty).ShouldEqual(expression);
                    }

                    return expression;
                }
            });
            var exp = _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression) exp).Value.ShouldEqual(2);
        }

        [Fact]
        public void ShouldOptimizeExpressions()
        {
            Expression expression = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(1));
            using var t = _compiler.AddComponent(new TestExpressionBuilderComponent
            {
                TryBuild = (_, _) => expression
            });
            var exp = _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression) exp).Value.ShouldEqual(2);

            expression = Expression.MakeBinary(ExpressionType.Add, Expression.Convert(Expression.Constant(1, typeof(object)), typeof(int)), Expression.Constant(1));
            exp = _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((ConstantExpression) exp).Value.ShouldEqual(2);

            expression = Expression.NewArrayInit(typeof(int), Expression.Constant(1), Expression.Constant(2));
            exp = _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            ((int[]?) ((ConstantExpression) exp).Value).ShouldEqual(new[] {1, 2});

            expression = Expression.MakeBinary(ExpressionType.Add, Expression.Parameter(typeof(int), "test"), Expression.Constant(1));
            exp = _compiler.GetComponents<IExpressionBuilderComponent>().TryBuild(new TestExpressionBuilderContext(), MemberExpressionNode.Empty)!;
            exp.ShouldEqual(expression);
        }
    }
}