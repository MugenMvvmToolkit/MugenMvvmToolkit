using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ExpressionCompilerComponent : AttachableComponentBase<IExpressionCompiler>, IExpressionCompilerComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private readonly IMetadataContextManager? _metadataContextManager;
        private IExpressionBuilderComponent[] _components;

        #endregion

        #region Constructors

        public ExpressionCompilerComponent(IMetadataContextManager? metadataContextManager = null)
        {
            _metadataContextManager = metadataContextManager;
            _components = Default.Array<IExpressionBuilderComponent>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionBuilderComponent, ExpressionCompilerComponent>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.LinqCompiler;

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new CompiledExpression(expression, metadata, _metadataContextManager) {ExpressionBuilders = _components};
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }

        #endregion
    }
}