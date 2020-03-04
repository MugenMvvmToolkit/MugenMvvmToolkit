using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingExpressionBuilderComponent[]? _expressionBuilderComponents;
        private IBindingHolderComponent[]? _holderComponents;
        private IBindingStateDispatcherComponent[]? _stateDispatcherComponents;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingExpressionBuilderComponent, BindingManager>((components, state, _) => state._expressionBuilderComponents = components, this);
            _componentTracker.AddListener<IBindingHolderComponent, BindingManager>((components, state, _) => state._holderComponents = components, this);
            _componentTracker.AddListener<IBindingStateDispatcherComponent, BindingManager>((components, state, _) => state._stateDispatcherComponents = components, this);
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (_expressionBuilderComponents == null)
                _componentTracker.Attach(this, metadata);
            var result = _expressionBuilderComponents!.TryBuildBindingExpression(expression, metadata);
            if (result.IsNullOrEmpty())
                BindingExceptionManager.ThrowCannotParseExpression(expression);
            return result;
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_holderComponents == null)
                _componentTracker.Attach(this, metadata);
            return _holderComponents!.TryGetBindings(target, path, metadata);
        }

        public IReadOnlyMetadataContext OnLifecycleChanged<TState>(IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            if (_stateDispatcherComponents == null)
                _componentTracker.Attach(this, metadata);
            return _stateDispatcherComponents!.OnLifecycleChanged(binding, lifecycleState, state, metadata).DefaultIfNull();
        }

        #endregion
    }
}