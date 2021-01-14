using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing
{
    public sealed class ExpressionParser : ComponentOwnerBase<IExpressionParser>, IExpressionParser, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        private readonly ComponentTracker _componentTracker;
        private ItemOrArray<IExpressionParserComponent> _components;

        public ExpressionParser(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionParserComponent, ExpressionParser>((components, state, _) => state._components = components, this);
        }

        public ItemOrIReadOnlyList<ExpressionParserResult> TryParse(object expression, IReadOnlyMetadataContext? metadata = null) =>
            _components.TryParse(this, expression, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            _componentTracker.OnComponentChanged(component, collection, metadata);
    }
}