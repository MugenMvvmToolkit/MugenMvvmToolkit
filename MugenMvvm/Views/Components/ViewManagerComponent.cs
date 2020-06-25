using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
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
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewManagerComponent : AttachableComponentBase<IViewManager>, IViewManagerComponent, IViewProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueProvider? _attachedValueProvider;
        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        private static readonly IMetadataContextKey<List<IView>, List<IView>> ViewsMetadataKey = MetadataContextKey.FromMember(ViewsMetadataKey, typeof(ViewManagerComponent), nameof(ViewsMetadataKey));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManagerComponent(IAttachedValueProvider? attachedValueProvider = null, IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _attachedValueProvider = attachedValueProvider;
            _componentCollectionProvider = componentCollectionProvider;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.Initializer;

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync<TRequest>(IViewModelViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (typeof(TRequest) != typeof(ViewModelViewRequest))
                    return null;

                var r = MugenExtensions.CastGeneric<TRequest, ViewModelViewRequest>(request);
                if (r.ViewModel == null || r.View == null)
                    return null;

                if (r.ViewModel is IComponentOwner componentOwner)
                {
                    var collection = componentOwner.Components;
                    return Task.FromResult(InitializeView(mapping, r.ViewModel, r.View, collection.Get<IView>(), collection, (c, view, m) => c.Add(view, m), (c, view, m) => c.Remove(view, m), metadata));
                }

                var list = r.ViewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?)null, (context, o) => new List<IView>(2));
                return Task.FromResult(InitializeView(mapping, r.ViewModel, r.View, list, list, (c, view, m) => c.Add(view), (c, view, m) => c.Remove(view), metadata));
            }
            catch (Exception e)
            {
                return Task.FromException<IView>(e);
            }
        }

        public Task? TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (!_attachedValueProvider.DefaultIfNull().TryGet<List<IView>>(view.Target, InternalConstant.ViewsValueKey, out var value) || !value.Contains(view))
                    return null;

                CleanupAsync(view, request, value, (list, v, m) =>
                {
                    list.Remove(v);
                    if (v.ViewModel is IComponentOwner componentOwner)
                        componentOwner.Components.Remove(v, m);
                    else
                        v.ViewModel.Metadata.Get(ViewsMetadataKey)?.Remove(v);
                }, metadata);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public ItemOrList<IView, IReadOnlyList<IView>> TryGetViews<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>())
                return default;
            if (request is IViewModelBase viewModel)
            {
                if (viewModel is IComponentOwner componentOwner)
                    return componentOwner.GetComponents<IView>();

                return GetViews(viewModel.GetMetadataOrDefault().Get(ViewsMetadataKey));
            }

            if (_attachedValueProvider.DefaultIfNull().TryGet<List<IView>>(request, InternalConstant.ViewsValueKey, out var value))
                return GetViews(value);
            return default;
        }

        #endregion

        #region Methods

        private static ItemOrList<IView, IReadOnlyList<IView>> GetViews(List<IView>? views)
        {
            if (views == null)
                return default;
            ItemOrList<IView, List<IView>> result = default;
            for (var i = 0; i < views.Count; i++)
                result.Add(views[i]);
            return result.Cast<IReadOnlyList<IView>>();
        }

        private IView InitializeView<TList>(IViewModelViewMapping mapping, IViewModelBase viewModel, object rawView,
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
                    CleanupAsync(oldView, (object?)null, collection, removeAction, metadata);
                }
            }

            var view = new View(mapping, rawView, viewModel, metadata, _componentCollectionProvider, _metadataContextProvider);
            Owner.OnLifecycleChanged(view, ViewLifecycleState.Initializing, viewModel, metadata);
            addAction(collection, view, metadata);
            _attachedValueProvider
                .DefaultIfNull()
                .GetOrAdd(rawView, InternalConstant.ViewsValueKey, rawView, (_, __) => new List<IView>())
                .Add(view);
            Owner.OnLifecycleChanged(view, ViewLifecycleState.Initialized, viewModel, metadata);
            return view;
        }

        private void CleanupAsync<TRequest, TList>(IView view, TRequest request, TList collection, Action<TList, IView, IReadOnlyMetadataContext?> removeAction, IReadOnlyMetadataContext? metadata)
        {
            Owner.OnLifecycleChanged(view, ViewLifecycleState.Clearing, request, metadata);
            removeAction(collection, view, metadata);
            if (_attachedValueProvider.DefaultIfNull().TryGet<List<IView>>(view.Target, InternalConstant.ViewsValueKey, out var value))
                value.Remove(view);
            Owner.OnLifecycleChanged(view, ViewLifecycleState.Cleared, request, metadata);
        }

        #endregion
    }
}