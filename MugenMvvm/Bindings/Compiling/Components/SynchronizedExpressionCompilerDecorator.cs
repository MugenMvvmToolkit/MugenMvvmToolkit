using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public class SynchronizedExpressionCompilerDecorator : ComponentDecoratorBase<IExpressionCompiler, IExpressionCompilerComponent>, IExpressionCompilerComponent,
        IHasCacheComponent<IExpressionCompiler>, IComponentCollectionDecorator<IHasCacheComponent<IExpressionCompiler>>, ISynchronizedComponent<IExpressionCompiler>
    {
        private ItemOrArray<IHasCacheComponent<IExpressionCompiler>> _cacheComponents;
        private readonly object _syncRoot;

        public SynchronizedExpressionCompilerDecorator(object? syncRoot = null, int priority = ComponentPriority.Synchronizer) : base(priority)
        {
            _syncRoot = syncRoot ?? this;
        }

        public object SyncRoot => _syncRoot;

        void IComponentCollectionDecorator<IHasCacheComponent<IExpressionCompiler>>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IHasCacheComponent<IExpressionCompiler>> components, IReadOnlyMetadataContext? metadata) =>
            _cacheComponents = this.Decorate(ref components);

        ICompiledExpression? IExpressionCompilerComponent.TryCompile(IExpressionCompiler compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return Components.TryCompile(compiler, expression, metadata);
            }
        }

        void IHasCacheComponent<IExpressionCompiler>.Invalidate(IExpressionCompiler owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                _cacheComponents.Invalidate(owner, state, metadata);
            }
        }
    }
}