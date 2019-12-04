using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Compiling
{
    public sealed class ExpressionCompiler : ComponentOwnerBase<IExpressionCompiler>, IExpressionCompiler
    {
        #region Constructors

        public ExpressionCompiler(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(expression, nameof(expression));
            var compilers = GetComponents<IExpressionCompilerComponent>(metadata);
            for (var i = 0; i < compilers.Length; i++)
            {
                var compiledExpression = compilers[i].TryCompile(expression, metadata);
                if (compiledExpression != null)
                    return compiledExpression;
            }

            BindingExceptionManager.ThrowCannotCompileExpression(expression);
            return null;
        }

        #endregion
    }
}