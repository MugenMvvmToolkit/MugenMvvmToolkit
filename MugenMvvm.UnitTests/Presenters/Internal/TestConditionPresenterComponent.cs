using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Presenters.Internal
{
    public class TestConditionPresenterComponent : IConditionPresenterComponent
    {
        #region Fields

        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public TestConditionPresenterComponent(IPresenter? presenter = null)
        {
            _presenter = presenter;
        }

        #endregion

        #region Properties

        public Func<IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, IReadOnlyMetadataContext?, bool>? CanClose { get; set; }

        public Func<IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, IReadOnlyMetadataContext?, bool>? CanShow { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionPresenterComponent.CanShow(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return CanShow?.Invoke(presenterComponent, results, request!, metadata) ?? false;
        }

        bool IConditionPresenterComponent.CanClose(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _presenter?.ShouldEqual(presenter);
            return CanClose?.Invoke(presenterComponent, results, request!, metadata) ?? false;
        }

        #endregion
    }
}