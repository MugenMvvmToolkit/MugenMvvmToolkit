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
    public class BindingManager : ComponentOwnerBase<IBindingManager>, IBindingManager,
        IComponentOwnerAddedCallback<IComponent<IBindingManager>>, IComponentOwnerRemovedCallback<IComponent<IBindingManager>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        protected IBindingBuilderComponent[] BindingBuilders;
        protected IBindingExpressionBuilderComponent[] ExpressionBuilders;
        protected IBindingHolderComponent[] Holders;
        protected IBindingStateDispatcherComponent[] StateDispatchers;

        #endregion

        #region Constructors

        public BindingManager(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
            BindingBuilders = Default.EmptyArray<IBindingBuilderComponent>();
            ExpressionBuilders = Default.EmptyArray<IBindingExpressionBuilderComponent>();
            Holders = Default.EmptyArray<IBindingHolderComponent>();
            StateDispatchers = Default.EmptyArray<IBindingStateDispatcherComponent>();
        }

        #endregion

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.ServiceIfNull();

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpression<T>(in T expression, IReadOnlyMetadataContext? metadata = null)
        {
            var result = BuildBindingExpressionInternal(expression, metadata);
            if (result.IsEmpty())
                BindingExceptionManager.ThrowCannotParseExpression(expression);
            return default;
        }

        public ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildBinding<T>(in T expression, object target, ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            var result = BuildBindingInternal(expression, target, sources, metadata);
            if (result.IsEmpty())
                BindingExceptionManager.ThrowCannotParseExpression(expression);
            return default;
        }

        public ItemOrList<IBinding?, IReadOnlyList<IBinding>> GetBindings(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            return GetBindingsInternal(target, path, metadata);
        }

        public IReadOnlyMetadataContext OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            return OnLifecycleChangedInternal(binding, lifecycle, metadata) ?? Default.Metadata;
        }

        void IComponentOwnerAddedCallback<IComponent<IBindingManager>>.OnComponentAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IBindingManager>>.OnComponentRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref BindingBuilders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref ExpressionBuilders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref Holders, this, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnAdded(ref StateDispatchers, this, collection, component, metadata);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref BindingBuilders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref ExpressionBuilders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref Holders, collection, component, metadata);
            MugenExtensions.ComponentTrackerOnRemoved(ref StateDispatchers, collection, component, metadata);
        }

        protected virtual ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> BuildBindingExpressionInternal<T>(in T expression,
            IReadOnlyMetadataContext? metadata = null)
        {
            var builders = ExpressionBuilders;
            for (var i = 0; i < builders.Length; i++)
            {
                if (builders[i] is IBindingExpressionBuilderComponent<T> builder)
                {
                    var result = builder.TryBuildBindingExpression(expression, metadata);
                    if (!result.IsEmpty())
                        return result;
                }
            }

            return default;
        }

        protected virtual ItemOrList<IBinding, IReadOnlyList<IBinding>> BuildBindingInternal<T>(in T expression, object target,
            ItemOrList<object?, IReadOnlyList<object?>> sources = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            var builders = BindingBuilders;
            for (var i = 0; i < builders.Length; i++)
            {
                if (builders[i] is IBindingBuilderComponent<T> builder)
                {
                    var result = builder.TryBuildBinding(expression, target, sources, metadata);
                    if (!result.IsEmpty())
                        return result;
                }
            }

            return default;
        }

        protected virtual ItemOrList<IBinding?, IReadOnlyList<IBinding>> GetBindingsInternal(object target, string? path = null, IReadOnlyMetadataContext? metadata = null)
        {
            var holders = Holders;
            if (holders.Length == 0)
                return default;
            if (holders.Length == 1)
                return holders[0].TryGetBindings(target, path, metadata);

            IBinding? item = null;
            List<IBinding>? list = null;
            for (var i = 0; i < holders.Length; i++)
            {
                var bindings = holders[i].TryGetBindings(target, path, metadata);
                if (bindings.IsEmpty())
                    continue;

                if (bindings.IsList)
                {
                    if (bindings.List.Count == 0)
                        continue;

                    if (list == null)
                    {
                        list = new List<IBinding>();
                        if (item != null)
                            list.Add(item);
                    }

                    list.AddRange(bindings.List);
                }
                else
                {
                    if (item == null)
                        item = bindings.Item;
                    else
                    {
                        if (list == null)
                            list = new List<IBinding> { item };
                        list.Add(bindings.Item);
                    }
                }
            }

            if (list == null)
                return new ItemOrList<IBinding, IReadOnlyList<IBinding>>(item);
            return new ItemOrList<IBinding, IReadOnlyList<IBinding>>(list);
        }

        protected virtual IReadOnlyMetadataContext? OnLifecycleChangedInternal(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            if (lifecycle == BindingLifecycleState.Disposed)
            {
                var holders = Holders;
                for (var i = 0; i < holders.Length; i++)
                {
                    if (holders[i].TryUnregister(binding, metadata))
                        break;
                }
            }

            var dispatchers = StateDispatchers;
            if (dispatchers.Length == 0)
                return null;
            if (dispatchers.Length == 1)
                return dispatchers[0].OnLifecycleChanged(binding, lifecycle, metadata);

            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < dispatchers.Length; i++)
            {
                var m = dispatchers[i].OnLifecycleChanged(binding, lifecycle, metadata);
                if (m == null || m.Count == 0)
                    continue;

                if (result == null)
                    result = m;
                else
                {
                    var r = result.ToNonReadonly(this, _metadataContextProvider);
                    r.Merge(m);
                    result = r;
                }
            }

            return result;
        }

        #endregion
    }
}