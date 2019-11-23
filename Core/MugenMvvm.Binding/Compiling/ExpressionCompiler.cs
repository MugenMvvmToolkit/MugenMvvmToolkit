using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Compiling
{
    public sealed class ExpressionCompiler : ComponentOwnerBase<IExpressionCompiler>, IExpressionCompiler, IComponentOwnerAddedCallback<IComponent<IExpressionCompiler>>,
        IComponentOwnerRemovedCallback<IComponent<IExpressionCompiler>>
    {
        #region Fields

        private IExpressionCompilerComponent[] _compilers;

        #endregion

        #region Constructors

        public ExpressionCompiler(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _compilers = Default.EmptyArray<IExpressionCompilerComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IExpressionCompiler>>.OnComponentAdded(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _compilers, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IExpressionCompiler>>.OnComponentRemoved(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _compilers, component);
        }

        public ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(expression, nameof(expression));
            var compilers = _compilers;
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