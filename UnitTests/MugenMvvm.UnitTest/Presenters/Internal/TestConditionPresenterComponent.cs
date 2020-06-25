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

        public Func<IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, Type, IReadOnlyMetadataContext?, bool>? CanClose { get; set; }

        public Func<IPresenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>>, object, Type, IReadOnlyMetadataContext?, bool>? CanShow { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionPresenterComponent.CanShow<TRequest>(IPresenterComponent presenter, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanShow?.Invoke(presenter, results, request!, typeof(TRequest), metadata) ?? false;
        }

        bool IConditionPresenterComponent.CanClose<TRequest>(IPresenterComponent presenter, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanClose?.Invoke(presenter, results, request!, typeof(TRequest), metadata) ?? false;
        }

        #endregion
    }
}