using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;

namespace MugenMvvm.Tests.Presentation
{
    public class TestPresenterComponent : IPresenterComponent, IHasPriority
    {
        public Func<IPresenter, object, IReadOnlyMetadataContext?, CancellationToken, ItemOrIReadOnlyList<IPresenterResult>>? TryClose { get; set; }

        public Func<IPresenter, object, IReadOnlyMetadataContext?, CancellationToken, ItemOrIReadOnlyList<IPresenterResult>>? TryShow { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IPresenterResult> IPresenterComponent.TryShow(IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) =>
            TryShow?.Invoke(presenter, request, metadata, cancellationToken) ?? default;

        ItemOrIReadOnlyList<IPresenterResult> IPresenterComponent.TryClose(IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) =>
            TryClose?.Invoke(presenter, request, metadata, cancellationToken) ?? default;
    }
}