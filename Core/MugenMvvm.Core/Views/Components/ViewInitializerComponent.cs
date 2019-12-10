using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
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

        public IReadOnlyList<IViewInfo> TryGetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
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
            {
                viewModel = Owner.GetComponents<IViewModelProviderViewManagerComponent>().TryGetViewModelForView(initializer, view!, metadata);
                if (viewModel == null)
                    ExceptionManager.ThrowObjectNotInitialized(Owner);
            }

            if (view == null)
            {
                view = Owner.GetComponents<IViewProviderComponent>().TryGetViewForViewModel(initializer, viewModel, metadata);
                if (view == null)
                    ExceptionManager.ThrowObjectNotInitialized(Owner);
            }

            var views = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?) null, (context, _) => new StringOrdinalLightDictionary<IViewInfo>(1));
            ViewInfo viewInfo;
            lock (views)
            {
                if (views.TryGetValue(initializer.Id, out var oldView))
                {
                    if (ReferenceEquals(oldView.View, view))
                        return new ViewInitializerResult(oldView, viewModel, metadata);

                    Owner.GetComponents<IViewManagerListener>().OnViewCleared(Owner, oldView, viewModel, metadata);
                }

                viewInfo = new ViewInfo(initializer, view, null, _metadataContextProvider);
                views[initializer.Id] = viewInfo;
            }

            Owner.GetComponents<IViewManagerListener>().OnViewInitialized(Owner, viewInfo, viewModel, metadata);
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
                Owner.GetComponents<IViewManagerListener>().OnViewCleared(Owner, viewInfo, viewModel, metadata);
            return Default.Metadata;
        }

        #endregion
    }
}