using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public sealed class ViewInitializerComponent : AttachableComponentBase<IViewManager>, IViewInitializerComponent, IViewProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private static readonly IMetadataContextKey<StringOrdinalLightDictionary<IView>> ViewsMetadataKey =
            MetadataContextKey.FromMember<StringOrdinalLightDictionary<IView>>(typeof(ViewInitializerComponent), nameof(ViewsMetadataKey));

        #endregion

        #region Constructors

        public ViewInitializerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.Initializer;

        #endregion

        #region Implementation of interfaces

        public Task<ViewInitializationResult>? TryInitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            if (viewModel == null || view == null)
                return null;
            var views = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?) null, (context, _) => new StringOrdinalLightDictionary<IView>(1));
            View resultView;
            lock (views)
            {
                if (views.TryGetValue(mapping.Id, out var oldView))
                {
                    if (ReferenceEquals(oldView.View, view))
                        return Task.FromResult(new ViewInitializationResult(oldView, viewModel, metadata));

                    Owner.GetComponents<IViewManagerListener>().OnViewCleared(Owner, oldView, viewModel, metadata);
                }

                resultView = new View(mapping, view, metadata, _metadataContextProvider);
                views[mapping.Id] = resultView;
            }

            Owner.GetComponents<IViewManagerListener>().OnViewInitialized(Owner, resultView, viewModel, metadata);
            return Task.FromResult(new ViewInitializationResult(resultView, viewModel, metadata));
        }

        public Task? TryCleanupAsync(IView view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                return null;

            var removed = false;
            var views = viewModel.Metadata.Get(ViewsMetadataKey!);
            if (views != null)
            {
                lock (views)
                {
                    removed = views.Remove(view.Mapping.Id);
                }
            }

            if (removed)
                Owner.GetComponents<IViewManagerListener>().OnViewCleared(Owner, view, viewModel, metadata);
            return Task.CompletedTask;
        }

        public IReadOnlyList<IView>? TryGetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var views = viewModel.Metadata.Get(ViewsMetadataKey!);
            if (views == null)
                return null;
            lock (views)
            {
                return views.ValuesToArray();
            }
        }

        #endregion
    }
}