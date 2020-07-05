using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        #region Properties

        public Func<IViewManager, object, Type, IReadOnlyMetadataContext?, ItemOrList<IView, IReadOnlyList<IView>>>? TryGetViews { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IView, IReadOnlyList<IView>> IViewProviderComponent.TryGetViews<TRequest>(IViewManager viewManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetViews?.Invoke(viewManager, request!, typeof(TRequest), metadata) ?? default;
        }

        #endregion
    }
}