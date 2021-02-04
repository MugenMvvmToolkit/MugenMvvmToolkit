using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using Should;

namespace MugenMvvm.UnitTests.Presentation.Internal
{
    public class TestPresenterComponent : IPresenterComponent, IHasPriority
    {
        private readonly IPresenter? _presenter;

        public TestPresenterComponent(IPresenter? presenter = null)
        {
            _presenter = presenter;
        }

        public Func<object, IReadOnlyMetadataContext?, CancellationToken, ItemOrIReadOnlyList<IPresenterResult>>? TryClose { get; set; }

        public Func<object, IReadOnlyMetadataContext?, CancellationToken, ItemOrIReadOnlyList<IPresenterResult>>? TryShow { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IPresenterResult> IPresenterComponent.TryShow(IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return TryShow?.Invoke(request, metadata, cancellationToken) ?? default;
        }

        ItemOrIReadOnlyList<IPresenterResult> IPresenterComponent.TryClose(IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return TryClose?.Invoke(request, metadata, cancellationToken) ?? default;
        }
    }
}