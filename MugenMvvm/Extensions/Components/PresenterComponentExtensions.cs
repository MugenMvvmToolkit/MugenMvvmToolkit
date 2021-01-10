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

        public static ItemOrIReadOnlyList<IPresenterResult> TryShow(this IPresenterComponent[] components, IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryShow(presenter, request, cancellationToken, metadata);
            var result = new ItemOrListEditor<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryShow(presenter, request, cancellationToken, metadata));
            return result.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<IPresenterResult> TryClose(this IPresenterComponent[] components, IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryClose(presenter, request, cancellationToken, metadata);
            var result = new ItemOrListEditor<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryClose(presenter, request, cancellationToken, metadata));
            return result.ToItemOrList();
        }

        public static IViewModelPresenterMediator? TryGetPresenterMediator(this IViewModelPresenterMediatorProviderComponent[] components, IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            for (var i = 0; i < components.Length; i++)
            {
                var mediator = components[i].TryGetPresenterMediator(presenter, viewModel, mapping, metadata);
                if (mediator != null)
                    return mediator;
            }

            return null;
        }

        public static IViewPresenter? TryGetViewPresenter(this IViewPresenterProviderComponent[] components, IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            for (var i = 0; i < components.Length; i++)
            {
                var viewPresenter = components[i].TryGetViewPresenter(presenter, viewModel, mapping, metadata);
                if (viewPresenter != null)
                    return viewPresenter;
            }

            return null;
        }

        #endregion
    }
}