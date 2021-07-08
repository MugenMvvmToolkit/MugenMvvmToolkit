using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presentation.Components
{
    public sealed class ViewModelPresenter : IPresenterComponent, IHasPriority
    {
        private readonly IViewManager? _viewManager;

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public int Priority { get; init; } = PresenterComponentPriority.Presenter;

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null)
                return default;

            var result = new ItemOrListEditor<IPresenterResult>(2);
            foreach (var mediator in TryGetMediators(presenter, viewModel, request, metadata))
                result.AddIfNotNull(mediator.TryShow(view, cancellationToken, metadata)!);

            return result.ToItemOrList();
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
            if (viewModel == null)
                return default;

            var dictionary = viewModel.GetOrDefault(InternalMetadata.Mediators);
            if (dictionary == null)
                return default;

            var result = new ItemOrListEditor<IPresenterResult>(2);
            lock (dictionary)
            {
                foreach (var mediator in dictionary)
                    result.AddIfNotNull(mediator.Value.TryClose(view, cancellationToken, metadata)!);
            }

            return result.ToItemOrList();
        }

        private ItemOrIReadOnlyList<IViewModelPresenterMediator> TryGetMediators(IPresenter presenter, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            var components = presenter.GetComponents<IViewModelPresenterMediatorProviderComponent>(metadata);
            if (components.Count == 0)
                return default;

            var result = new ItemOrListEditor<IViewModelPresenterMediator>(2);
            var dictionary = viewModel.Metadata.GetOrAdd(InternalMetadata.Mediators, viewModel, (_, _, _) => new Dictionary<string, IViewModelPresenterMediator>());
            lock (dictionary)
            {
                foreach (var mapping in _viewManager.DefaultIfNull(viewModel).GetMappings(request, metadata))
                {
                    if (!dictionary.TryGetValue(mapping.Id, out var mediator))
                    {
                        mediator = components.TryGetPresenterMediator(presenter, viewModel, mapping, metadata)!;
                        if (mediator == null)
                            continue;

                        dictionary[mapping.Id] = mediator;
                    }

                    result.Add(mediator);
                }
            }

            return result.ToItemOrList();
        }
    }
}