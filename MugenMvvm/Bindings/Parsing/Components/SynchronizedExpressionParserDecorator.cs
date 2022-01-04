using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Bindings.Parsing.Components
{
    public class SynchronizedExpressionParserDecorator : ComponentDecoratorBase<IExpressionParser, IExpressionParserComponent>, IExpressionParserComponent,
        IHasCacheComponent<IExpressionParser>, IComponentCollectionDecorator<IHasCacheComponent<IExpressionParser>>, ISynchronizedComponent<IExpressionParser>
    {
        private ItemOrArray<IHasCacheComponent<IExpressionParser>> _cacheComponents;
        private readonly object _syncRoot;

        public SynchronizedExpressionParserDecorator(object? syncRoot = null, int priority = ComponentPriority.Synchronizer) : base(priority)
        {
            _syncRoot = syncRoot ?? this;
        }

        public object SyncRoot => _syncRoot;

        void IComponentCollectionDecorator<IHasCacheComponent<IExpressionParser>>.Decorate(IComponentCollection collection,
            ref ItemOrListEditor<IHasCacheComponent<IExpressionParser>> components, IReadOnlyMetadataContext? metadata) =>
            _cacheComponents = this.Decorate(ref components);

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                return Components.TryParse(parser, expression, metadata);
            }
        }

        void IHasCacheComponent<IExpressionParser>.Invalidate(IExpressionParser owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            lock (_syncRoot)
            {
                _cacheComponents.Invalidate(owner, state, metadata);
            }
        }
    }
}