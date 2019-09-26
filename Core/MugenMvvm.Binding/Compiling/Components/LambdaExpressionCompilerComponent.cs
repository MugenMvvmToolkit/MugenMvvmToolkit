using System;
using System.Linq.Expressions;
using System.Threading;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class LambdaExpressionCompilerComponent : IExpressionCompilerComponent
    {
        #region Fields

        private readonly ComponentTracker<ICompiler, IExpressionCompiler> _componentTracker;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public LambdaExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _componentTracker = new ComponentTracker<ICompiler, IExpressionCompiler>();
        }

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        protected virtual ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new LambdaCompiledExpression(this, expression, metadata);
        }

        #endregion

        #region Nested types

//        private 

        private sealed class LambdaCompiledExpression : ICompiledExpression, IContext
        {
            #region Fields

            private readonly LambdaExpressionCompilerComponent _compiler;
            private readonly IExpressionNode _expression;
            private IReadOnlyMetadataContext? _metadata;

            #endregion

            #region Constructors

            public LambdaCompiledExpression(LambdaExpressionCompilerComponent compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
            {
                _compiler = compiler;
                _expression = expression;
                _metadata = metadata;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _compiler._metadataContextProvider), null);
                    return (IMetadataContext) _metadata!;
                }
            }

            #endregion

            #region Implementation of interfaces

            public object? Invoke(object?[] values, IReadOnlyMetadataContext metadata)
            {
                throw new NotImplementedException();
            }

            public Expression Compile(IExpressionNode expression)
            {
                var components = _compiler._componentTracker.GetComponents();
                foreach (var component in components)
                {
                    var compile = component.TryCompile(this, expression);
                    if (compile != null)
                        return compile;
                }

                BindingExceptionManager.CannotCompileExpression(expression);
                return null!;
            }

            #endregion
        }

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            Expression Compile(IExpressionNode expression);
        }

        public interface ICompiler : IComponent<IExpressionCompiler>
        {
            Expression? TryCompile(IContext context, IExpressionNode expression);
        }

        #endregion
    }
}