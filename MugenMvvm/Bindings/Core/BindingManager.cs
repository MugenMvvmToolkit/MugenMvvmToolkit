using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager, IHasComponentAddedHandler, IHasComponentRemovedHandler
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingExpressionParserComponent[] _expressionBuilderComponents;
        private IBindingHolderComponent[] _holderComponents;
        private IBindingLifecycleListener[] _stateDispatcherComponents;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _expressionBuilderComponents = Default.Array<IBindingExpressionParserComponent>();
            _holderComponents = Default.Array<IBindingHolderComponent>();
            _stateDispatcherComponents = Default.Array<IBindingLifecycleListener>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingExpressionParserComponent, BindingManager>((components, state, _) => state._expressionBuilderComponents = components, this);
            _componentTracker.AddListener<IBindingHolderComponent, BindingManager>((components, state, _) => state._holderComponents = components, this);
            _componentTracker.AddListener<IBindingLifecycleListener, BindingManager>((components, state, _) => state._stateDispatcherComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(object expression, IReadOnlyMetadataContext? metadata = null) =>
            _expressionBuilderComponents.TryParseBindingExpression(this, expression, metadata);

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null) => _holderComponents.TryGetBindings(this, target, path, metadata);

        public void OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null) =>
            _stateDispatcherComponents.OnLifecycleChanged(this, binding, lifecycleState, state, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => _componentTracker.OnComponentChanged(component, collection, metadata);

        #endregion
    }
}