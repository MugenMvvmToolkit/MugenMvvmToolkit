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