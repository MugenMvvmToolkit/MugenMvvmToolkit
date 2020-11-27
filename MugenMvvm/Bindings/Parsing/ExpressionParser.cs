using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing
{
    public sealed class ExpressionParser : ComponentOwnerBase<IExpressionParser>, IExpressionParser, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IExpressionParserComponent[] _components;

        #endregion

        #region Constructors

        public ExpressionParser(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _components = Default.Array<IExpressionParserComponent>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionParserComponent, ExpressionParser>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(object expression, IReadOnlyMetadataContext? metadata = null) => _components.TryParse(this, expression, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        #endregion
    }
}