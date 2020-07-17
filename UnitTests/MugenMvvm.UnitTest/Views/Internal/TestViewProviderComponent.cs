using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public TestViewProviderComponent(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, ItemOrList<IView, IReadOnlyList<IView>>>? TryGetViews { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<IView, IReadOnlyList<IView>> IViewProviderComponent.TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            return TryGetViews?.Invoke(request!, metadata) ?? default;
        }

        #endregion
    }
}