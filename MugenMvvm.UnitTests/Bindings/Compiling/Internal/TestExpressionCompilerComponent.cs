using System;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
{
    public class TestExpressionCompilerComponent : IExpressionCompilerComponent
    {
        private readonly IExpressionCompiler? _compiler;

        public TestExpressionCompilerComponent(IExpressionCompiler? compiler = null)
        {
            _compiler = compiler;
        }

        public Func<IExpressionNode, IReadOnlyMetadataContext?, ICompiledExpression?>? TryCompile { get; set; }

        ICompiledExpression? IExpressionCompilerComponent.TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            _compiler?.ShouldEqual(compiler);
            return TryCompile?.Invoke(expression, metadata);
        }
    }
}