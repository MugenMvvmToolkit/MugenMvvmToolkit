using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, IReadOnlyList<IView>?>? TryGetViews { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IView>? IViewProviderComponent.TryGetViews<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetViews?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}