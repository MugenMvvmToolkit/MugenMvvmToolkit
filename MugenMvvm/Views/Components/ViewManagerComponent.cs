using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewManagerComponent : IViewManagerComponent, IViewProviderComponent, IHasPriority
    {
        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IComponentCollectionManager? _componentCollectionManager;

        [Preserve(Conditional = true)]
        public ViewManagerComponent(IAttachedValueManager? attachedValueManager = null, IComponentCollectionManager? componentCollectionManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _componentCollectionManager = componentCollectionManager;
        }

        public int Priority { get; set; } = ViewComponentPriority.Initializer;

        private static ItemOrIReadOnlyList<IView> GetViews(List<IView>? views)
        {
            if (views == null)
                return default;
            var result = new ItemOrListEditor<IView>();
            for (var i = 0; i < views.Count; i++)
                result.Add(views[i]);
            return result.ToItemOrList();
        }

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null || view == null)
                return default;

            if (viewModel is IComponentOwner componentOwner)
            {
                var collection = componentOwner.Components;
                return new ValueTask<IView?>(InitializeView(viewManager, mapping, viewModel, view, collection.Get<IView>(), collection, (c, v, m) => c.Add(v, m),
                    (c, v, m) => c.Remove(v, m), metadata));
            }

            var list = viewModel.Metadata.GetOrAdd(InternalMetadata.Views, (object?) null, (_, _, _) => new List<IView>(2));
            return new ValueTask<IView?>(InitializeView(viewManager, mapping, viewModel, view, list, list, (c, v, m) => c.Add(v), (c, v, m) => c.Remove(v), metadata));
        }

        public ValueTask<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!view.Target.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.ViewsValueKey, out var v)
                || !(v is List<IView> value) || !value.Contains(view))
                return default;

            Cleanup(viewManager, view, state, value, (list, item, m) =>
            {
                list.Remove(item);
                if (item.ViewModel is IComponentOwner componentOwner)
                    componentOwner.Components.Remove(item, m);
                else
                    item.ViewModel.Metadata.Get(InternalMetadata.Views)?.Remove(item);
            }, metadata);
            return new ValueTask<bool>(true);
        }

        public ItemOrIReadOnlyList<IView> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel != null)
            {
                if (viewModel is IComponentOwner componentOwner)
                    return componentOwner.GetComponents<IView>(metadata);

                return GetViews(viewModel.GetOrDefault(InternalMetadata.Views));
            }

            if (view != null && view.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.ViewsValueKey, out var value))
                return GetViews((List<IView>?) value);
            return default;
        }

        private IView InitializeView<TList>(IViewManager viewManager, IViewMapping mapping, IViewModelBase viewModel, object rawView,
            ItemOrIReadOnlyList<IView> views, TList collection, Action<TList, IView, IReadOnlyMetadataContext?> addAction,
            Action<TList, IView, IReadOnlyMetadataContext?> removeAction, IReadOnlyMetadataContext? metadata) where TList : class
        {
            foreach (var oldView in views)
            {
                if (oldView.Mapping.Id == mapping.Id)
                {
                    if (oldView.Target == rawView)
                        return oldView;
                    Cleanup(viewManager, oldView, null, collection, removeAction, metadata);
                }
            }

            var view = new View(mapping, rawView, viewModel, metadata, _componentCollectionManager);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, viewModel, metadata);
            addAction(collection, view, metadata);
            rawView.AttachedValues(metadata, _attachedValueManager).GetOrAdd(InternalConstant.ViewsValueKey, rawView, (_, _) => new List<IView>()).Add(view);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, viewModel, metadata);
            return view;
        }

        private void Cleanup<TList>(IViewManager viewManager, IView view, object? state, TList collection, Action<TList, IView, IReadOnlyMetadataContext?> removeAction,
            IReadOnlyMetadataContext? metadata)
        {
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, state, metadata);
            removeAction(collection, view, metadata);
            if (view.Target.AttachedValues(metadata, _attachedValueManager).TryGet(InternalConstant.ViewsValueKey, out var value))
                (value as List<IView>)?.Remove(view);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, state, metadata);
        }
    }
}