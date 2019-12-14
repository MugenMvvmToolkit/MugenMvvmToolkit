using MugenMvvm.Binding.Extensions.Components;
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
            var result = GetComponents<IExpressionCompilerComponent>(metadata).TryCompile(expression, metadata);
            if (result == null)
                BindingExceptionManager.ThrowCannotCompileExpression(expression);
            return result;
        }

        #endregion
    }
}