using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestPresenterComponent : IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public TestPresenterComponent(IPresenter? presenter = null)
        {
            _presenter = presenter;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, CancellationToken, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>>? TryClose { get; set; }

        public Func<object, IReadOnlyMetadataContext?, CancellationToken, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>>? TryShow { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> IPresenterComponent.TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return TryShow?.Invoke(request, metadata, cancellationToken) ?? default;
        }

        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> IPresenterComponent.TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return TryClose?.Invoke(request, metadata, cancellationToken) ?? default;
        }

        #endregion
    }
}