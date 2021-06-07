using System;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Compiling
{
    public class TestExpressionCompilerComponent : IExpressionCompilerComponent
    {
        public Func<IExpressionCompiler, IExpressionNode, IReadOnlyMetadataContext?, ICompiledExpression?>? TryCompile { get; set; }

        ICompiledExpression? IExpressionCompilerComponent.TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata) =>
            TryCompile?.Invoke(compiler, expression, metadata);
    }
}