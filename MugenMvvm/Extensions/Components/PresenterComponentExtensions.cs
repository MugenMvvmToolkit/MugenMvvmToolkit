using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        public static ItemOrIReadOnlyList<IPresenterResult> TryShow(this ItemOrArray<IPresenterComponent> components, IPresenter presenter, object request,
            CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryShow(presenter, request, cancellationToken, metadata);
            var result = new ItemOrListEditor<IPresenterResult>();
            foreach (var c in components)
                result.AddRange(c.TryShow(presenter, request, cancellationToken, metadata));

            return result.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<IPresenterResult> TryClose(this ItemOrArray<IPresenterComponent> components, IPresenter presenter, object request,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryClose(presenter, request, cancellationToken, metadata);
            var result = new ItemOrListEditor<IPresenterResult>();
            foreach (var c in components)
                result.AddRange(c.TryClose(presenter, request, cancellationToken, metadata));

            return result.ToItemOrList();
        }

        public static IViewModelPresenterMediator? TryGetPresenterMediator(this ItemOrArray<IViewModelPresenterMediatorProviderComponent> components, IPresenter presenter,
            IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            foreach (var c in components)
            {
                var mediator = c.TryGetPresenterMediator(presenter, viewModel, mapping, metadata);
                if (mediator != null)
                    return mediator;
            }

            return null;
        }

        public static IViewPresenterMediator? TryGetViewPresenter(this ItemOrArray<IViewPresenterMediatorProviderComponent> components, IPresenter presenter, IViewModelBase viewModel,
            IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            foreach (var c in components)
            {
                var viewPresenter = c.TryGetViewPresenter(presenter, viewModel, mapping, metadata);
                if (viewPresenter != null)
                    return viewPresenter;
            }

            return null;
        }
    }
}