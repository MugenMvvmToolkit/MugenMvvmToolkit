using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingExpressionParserComponent[]? _expressionBuilderComponents;
        private IBindingHolderComponent[]? _holderComponents;
        private IBindingLifecycleDispatcherComponent[]? _stateDispatcherComponents;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingExpressionParserComponent, BindingManager>((components, state, _) => state._expressionBuilderComponents = components, this);
            _componentTracker.AddListener<IBindingHolderComponent, BindingManager>((components, state, _) => state._holderComponents = components, this);
            _componentTracker.AddListener<IBindingLifecycleDispatcherComponent, BindingManager>((components, state, _) => state._stateDispatcherComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(object expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (_expressionBuilderComponents == null)
                _componentTracker.Attach(this, metadata);
            return _expressionBuilderComponents!.TryParseBindingExpression(this, expression, metadata);
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_holderComponents == null)
                _componentTracker.Attach(this, metadata);
            return _holderComponents!.TryGetBindings(this, target, path, metadata);
        }

        public void OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null)
        {
            if (_stateDispatcherComponents == null)
                _componentTracker.Attach(this, metadata);
            _stateDispatcherComponents!.OnLifecycleChanged(this, binding, lifecycleState, state, metadata);
        }

        #endregion
    }
}