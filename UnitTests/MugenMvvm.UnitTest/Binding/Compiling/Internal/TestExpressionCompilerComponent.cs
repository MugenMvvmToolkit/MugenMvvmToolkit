using System;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Compiling.Internal
{
    public class TestExpressionCompilerComponent : IExpressionCompilerComponent
    {
        #region Properties

        public Func<IExpressionCompiler, IExpressionNode, IReadOnlyMetadataContext?, ICompiledExpression?>? TryCompile { get; set; }

        #endregion

        #region Implementation of interfaces

        ICompiledExpression? IExpressionCompilerComponent.TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return TryCompile?.Invoke(compiler, expression, metadata);
        }

        #endregion
    }
}