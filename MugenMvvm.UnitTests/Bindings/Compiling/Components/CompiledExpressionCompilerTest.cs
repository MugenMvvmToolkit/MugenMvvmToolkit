using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class CompiledExpressionCompilerTest : UnitTestBase
    {
        [Fact]
        public void TryCompileShouldReturnCompiledExpressionWithComponents()
        {
            var expressionCompiler = new ExpressionCompiler(ComponentCollectionManager);
            expressionCompiler.AddComponent(new CompiledExpressionCompiler());

            var compiledExpression = (CompiledExpression) expressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();

            var testBuilder = new TestExpressionBuilderComponent();
            expressionCompiler.AddComponent(testBuilder);

            compiledExpression = (CompiledExpression) expressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.Single().ShouldEqual(testBuilder);

            expressionCompiler.RemoveComponent(testBuilder);
            compiledExpression = (CompiledExpression) expressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();
        }
    }
}