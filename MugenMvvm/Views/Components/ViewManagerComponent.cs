﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
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
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewManagerComponent : IViewManagerComponent, IViewProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly IComponentCollectionManager? _componentCollectionManager;

        private static readonly IMetadataContextKey<List<IView>, List<IView>> ViewsMetadataKey = MetadataContextKey.FromMember(ViewsMetadataKey, typeof(ViewManagerComponent), nameof(ViewsMetadataKey));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManagerComponent(IAttachedValueManager? attachedValueManager = null, IComponentCollectionManager? componentCollectionManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.Initializer;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                if (viewModel == null || view == null)
                    return null;

                if (viewModel is IComponentOwner componentOwner)
                {
                    var collection = componentOwner.Components;
                    return Task.FromResult(InitializeView(viewManager, mapping, viewModel, view, collection.Get<IView>(), collection, (c, v, m) => c.Add(v, m), (c, v, m) => c.Remove(v, m), metadata));
                }

                var list = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?)null, (context, o) => new List<IView>(2));
                return Task.FromResult(InitializeView(viewManager, mapping, viewModel, view, list, list, (c, v, m) => c.Add(v), (c, v, m) => c.Remove(v), metadata));
            }
            catch (Exception e)
            {
                return Task.FromException<IView>(e);
            }
        }

        public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (!_attachedValueManager.DefaultIfNull().TryGetAttachedValues(view.Target).TryGet(InternalConstant.ViewsValueKey, out var v)
                    || !(v is List<IView> value) || !value.Contains(view))
                    return null;

                CleanupAsync(viewManager, view, state, value, (list, item, m) =>
                {
                    list.Remove(item);
                    if (item.ViewModel is IComponentOwner componentOwner)
                        componentOwner.Components.Remove(item, m);
                    else
                        item.ViewModel.Metadata.Get(ViewsMetadataKey)?.Remove(item);
                }, metadata);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public ItemOrList<IView, IReadOnlyList<IView>> TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);

            if (viewModel != null)
            {
                if (viewModel is IComponentOwner componentOwner)
                    return ItemOrList.FromListToReadOnly(componentOwner.GetComponents<IView>());

                return GetViews(viewModel.GetMetadataOrDefault().Get(ViewsMetadataKey));
            }

            if (view != null && _attachedValueManager.DefaultIfNull().TryGetAttachedValues(view, metadata).TryGet(InternalConstant.ViewsValueKey, out var value))
                return GetViews((List<IView>?)value);
            return default;
        }

        #endregion

        #region Methods

        private static ItemOrList<IView, IReadOnlyList<IView>> GetViews(List<IView>? views)
        {
            if (views == null)
                return default;
            ItemOrListEditor<IView, List<IView>> result = ItemOrListEditor.Get<IView>();
            for (var i = 0; i < views.Count; i++)
                result.Add(views[i]);
            return result.ToItemOrList<IReadOnlyList<IView>>();
        }

        private IView InitializeView<TList>(IViewManager viewManager, IViewMapping mapping, IViewModelBase viewModel, object rawView,
            IList<IView> views, TList collection, Action<TList, IView, IReadOnlyMetadataContext?> addAction,
            Action<TList, IView, IReadOnlyMetadataContext?> removeAction, IReadOnlyMetadataContext? metadata) where TList : class
        {
            for (var i = 0; i < views.Count; i++)
            {
                var oldView = views[i];
                if (oldView.Mapping.Id == mapping.Id)
                {
                    if (ReferenceEquals(oldView.Target, rawView))
                        return oldView;
                    CleanupAsync(viewManager, oldView, null, collection, removeAction, metadata);
                }
            }

            var view = new View(mapping, rawView, viewModel, metadata, _componentCollectionManager);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, viewModel, metadata);
            addAction(collection, view, metadata);
            _attachedValueManager.DefaultIfNull().TryGetAttachedValues(rawView, metadata).GetOrAdd(InternalConstant.ViewsValueKey, rawView, (_, __) => new List<IView>()).Add(view);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, viewModel, metadata);
            return view;
        }

        private void CleanupAsync<TList>(IViewManager viewManager, IView view, object? state, TList collection, Action<TList, IView, IReadOnlyMetadataContext?> removeAction, IReadOnlyMetadataContext? metadata)
        {
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, state, metadata);
            removeAction(collection, view, metadata);
            if (_attachedValueManager.DefaultIfNull().TryGetAttachedValues(view.Target, metadata).TryGet(InternalConstant.ViewsValueKey, out var value))
                (value as List<IView>)?.Remove(view);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, state, metadata);
        }

        #endregion
    }
}