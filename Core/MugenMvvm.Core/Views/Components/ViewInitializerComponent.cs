using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewInitializerComponent : AttachableComponentBase<IViewManager>, IViewInitializerComponent, IViewInfoProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private static readonly IMetadataContextKey<StringOrdinalLightDictionary<IViewInfo>> ViewsMetadataKey =
            MetadataContextKey.FromMember<StringOrdinalLightDictionary<IViewInfo>>(typeof(ViewInitializerComponent), nameof(ViewsMetadataKey));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewInitializerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ViewInitializer;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var views = viewModel.Metadata.Get(ViewsMetadataKey);
            if (views == null)
                return Default.EmptyArray<IViewInfo>();
            lock (views)
            {
                return views.ValuesToArray();
            }
        }

        public IViewInitializerResult? TryInitialize(IViewInitializer initializer, IViewModelBase? viewModel, object? view, IMetadataContext metadata)
        {
            if (view == null && viewModel == null)
                return null;
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (viewModel == null)
                viewModel = GetViewModelForView(initializer, view!, metadata);
            if (view == null)
                view = GetViewForViewModel(initializer, viewModel, metadata);
            var views = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?) null, (context, _) => new StringOrdinalLightDictionary<IViewInfo>(1));
            ViewInfo viewInfo;
            lock (views)
            {
                if (views.TryGetValue(initializer.Id, out var oldView))
                {
                    if (ReferenceEquals(oldView.View, view))
                        return new ViewInitializerResult(oldView, viewModel, metadata);

                    OnViewCleared(oldView, viewModel, metadata);
                }

                viewInfo = new ViewInfo(initializer, view, null, _metadataContextProvider);
                views[initializer.Id] = viewInfo;
            }

            OnViewInitialized(viewInfo, viewModel, metadata);
            return new ViewInitializerResult(viewInfo, viewModel, metadata);
        }

        public IReadOnlyMetadataContext? TryCleanup(IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var removed = false;
            var views = viewModel.Metadata.Get(ViewsMetadataKey);
            if (views != null)
            {
                lock (views)
                {
                    removed = views.Remove(initializer.Id);
                }
            }

            if (removed)
                OnViewCleared(viewInfo, viewModel, metadata);
            return Default.Metadata;
        }

        #endregion

        #region Methods

        private object GetViewForViewModel(IViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IViewProviderComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var view = components[i].TryGetViewForViewModel(initializer, viewModel, metadata);
                if (view != null)
                    return view;
            }

            ExceptionManager.ThrowObjectNotInitialized(Owner, components);
            return null;
        }

        private IViewModelBase GetViewModelForView(IViewInitializer initializer, object view, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IViewModelProviderViewManagerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = components[i].TryGetViewModelForView(initializer, view, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            ExceptionManager.ThrowObjectNotInitialized(Owner, components);
            return null;
        }

        private void OnViewInitialized(IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IViewManagerListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnViewInitialized(Owner, viewInfo, viewModel, metadata);
        }

        private void OnViewCleared(IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = Owner.GetComponents<IViewManagerListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnViewCleared(Owner, viewInfo, viewModel, metadata);
        }

        #endregion
    }
}