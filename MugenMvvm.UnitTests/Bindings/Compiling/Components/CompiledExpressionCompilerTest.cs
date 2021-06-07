using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class CompiledExpressionCompilerTest : UnitTestBase
    {
        public CompiledExpressionCompilerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ExpressionCompiler.AddComponent(new CompiledExpressionCompiler());
        }

        [Fact]
        public void TryCompileShouldReturnCompiledExpressionWithComponents()
        {
            var compiledExpression = (CompiledExpression)ExpressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();

            var testBuilder = new TestExpressionBuilderComponent();
            ExpressionCompiler.AddComponent(testBuilder);

            compiledExpression = (CompiledExpression)ExpressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.Single().ShouldEqual(testBuilder);

            ExpressionCompiler.RemoveComponent(testBuilder);
            compiledExpression = (CompiledExpression)ExpressionCompiler.TryCompile(ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();
        }

        protected override IExpressionCompiler GetExpressionCompiler() => new ExpressionCompiler(ComponentCollectionManager);
    }
}