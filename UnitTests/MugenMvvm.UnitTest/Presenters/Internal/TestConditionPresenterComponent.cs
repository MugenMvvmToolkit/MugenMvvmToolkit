using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestConditionPresenterComponent : IConditionPresenterComponent
    {
        #region Properties

        public Func<IPresenter, IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, Type, IReadOnlyMetadataContext?, bool>? CanClose { get; set; }

        public Func<IPresenter, IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, Type, IReadOnlyMetadataContext?, bool>? CanShow { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionPresenterComponent.CanShow<TRequest>(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanShow?.Invoke(presenter, presenterComponent, results, request!, typeof(TRequest), metadata) ?? false;
        }

        bool IConditionPresenterComponent.CanClose<TRequest>(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanClose?.Invoke(presenter, presenterComponent, results, request!, typeof(TRequest), metadata) ?? false;
        }

        #endregion
    }
}