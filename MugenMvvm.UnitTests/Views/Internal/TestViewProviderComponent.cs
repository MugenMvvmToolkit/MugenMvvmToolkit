﻿using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using Should;

namespace MugenMvvm.UnitTests.Views.Internal
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        private readonly IViewManager? _viewManager;

        public TestViewProviderComponent(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public Func<object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IView>>? TryGetViews { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IView> IViewProviderComponent.TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            return TryGetViews?.Invoke(request!, metadata) ?? default;
        }
    }
}