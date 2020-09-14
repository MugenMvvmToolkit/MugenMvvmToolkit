using System;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Binding.Compiling.Internal
{
    public class TestExpressionCompilerComponent : IExpressionCompilerComponent
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;

        #endregion

        #region Constructors

        public TestExpressionCompilerComponent(IExpressionCompiler? compiler = null)
        {
            _compiler = compiler;
        }

        #endregion

        #region Properties

        public Func<IExpressionNode, IReadOnlyMetadataContext?, ICompiledExpression?>? TryCompile { get; set; }

        #endregion

        #region Implementation of interfaces

        ICompiledExpression? IExpressionCompilerComponent.TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            _compiler?.ShouldEqual(compiler);
            return TryCompile?.Invoke(expression, metadata);
        }

        #endregion
    }
}