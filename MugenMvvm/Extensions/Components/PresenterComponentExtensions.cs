using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static ItemOrIReadOnlyList<IPresenterResult> TryShow(this ItemOrArray<IPresenterComponent> components, IPresenter presenter, object request, CancellationToken cancellationToken,
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

        public static ItemOrIReadOnlyList<IPresenterResult> TryClose(this ItemOrArray<IPresenterComponent> components, IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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

        public static IViewModelPresenterMediator? TryGetPresenterMediator(this ItemOrArray<IViewModelPresenterMediatorProviderComponent> components, IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
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

        public static IViewPresenter? TryGetViewPresenter(this ItemOrArray<IViewPresenterProviderComponent> components, IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
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

        #endregion
    }
}