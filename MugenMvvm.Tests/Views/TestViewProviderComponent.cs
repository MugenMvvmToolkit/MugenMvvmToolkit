using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Tests.Views
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        public Func<IViewManager, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IView>>? TryGetViews { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IView> IViewProviderComponent.TryGetViews(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetViews?.Invoke(viewManager, request!, metadata) ?? default;
    }
}