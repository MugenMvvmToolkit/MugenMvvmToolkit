using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Presenters;

namespace MugenMvvm.UnitTest.Presenters.Internal
{
    public class TestConditionPresenterComponent : IConditionPresenterComponent
    {
        #region Properties

        public Func<IPresenterComponent, object, Type, IReadOnlyMetadataContext?, bool>? CanShow { get; set; }

        public Func<IPresenterComponent, IReadOnlyList<PresenterResult>, object, Type, IReadOnlyMetadataContext?, bool>? CanClose { get; set; }

        public Func<IPresenterComponent, IReadOnlyList<PresenterResult>, object, Type, IReadOnlyMetadataContext?, bool>? CanRestore { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionPresenterComponent.CanShow<TRequest>(IPresenterComponent presenter, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanShow?.Invoke(presenter, request!, typeof(TRequest), metadata) ?? false;
        }

        bool IConditionPresenterComponent.CanClose<TRequest>(IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanClose?.Invoke(presenter, results, request!, typeof(TRequest), metadata) ?? false;
        }

        bool IConditionPresenterComponent.CanRestore<TRequest>(IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return CanRestore?.Invoke(presenter, results, request!, typeof(TRequest), metadata) ?? false;
        }

        #endregion
    }
}