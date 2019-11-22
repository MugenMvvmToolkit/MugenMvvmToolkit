using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager, IComponentOwnerAddedCallback<IComponent<IBindingManager>>,
        IComponentOwnerRemovedCallback<IComponent<IBindingManager>>
    {
        #region Fields

        private IBindingExpressionBuilderComponent[] _expressionBuilders;
        private IBindingHolderComponent[] _holders;
        private IBindingStateDispatcherComponent[] _stateDispatchers;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            _expressionBuilders = Default.EmptyArray<IBindingExpressionBuilderComponent>();
            _holders = Default.EmptyArray<IBindingHolderComponent>();
            _stateDispatchers = Default.EmptyArray<IBindingStateDispatcherComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata = null)
        {
            var builders = _expressionBuilders;
            for (var i = 0; i < builders.Length; i++)
            {
                var result = builders[i].TryBuildBindingExpression(expression, metadata);
                if (!result.IsNullOrEmpty())
                    return result;
            }

            BindingExceptionManager.ThrowCannotParseExpression(expression);
            return default;
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var holders = _holders;
            if (holders.Length == 0)
                return default;
            if (holders.Length == 1)
                return holders[0].TryGetBindings(target, path, metadata);

            IBinding? item = null;
            List<IBinding>? list = null;
            for (var i = 0; i < holders.Length; i++)
                holders[i].TryGetBindings(target, path, metadata).Merge(ref item, ref list);

            if (list == null)
                return new ItemOrList<IBinding, IReadOnlyList<IBinding>>(item);
            return new ItemOrList<IBinding, IReadOnlyList<IBinding>>(list);
        }

        public IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            var dispatchers = _stateDispatchers;
            if (dispatchers.Length == 0)
                return Default.Metadata;
            if (dispatchers.Length == 1)
                return dispatchers[0].OnLifecycleChanged(binding, lifecycle, metadata) ?? Default.Metadata;

            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < dispatchers.Length; i++)
            {
                var m = dispatchers[i].OnLifecycleChanged(binding, lifecycle, metadata);
                m.Aggregate(ref result);
            }

            return result ?? Default.Metadata;
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingManager>>.OnComponentAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _expressionBuilders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _holders, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref _stateDispatchers, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IBindingManager>>.OnComponentRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _expressionBuilders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _holders, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref _stateDispatchers, component);
        }

        #endregion
    }
}