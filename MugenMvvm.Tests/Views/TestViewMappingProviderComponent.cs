using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Tests.Views
{
    public class TestViewMappingProviderComponent : IViewMappingProviderComponent, IHasPriority
    {
        public Func<IViewManager, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<IViewMapping>>? TryGetMappings { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<IViewMapping> IViewMappingProviderComponent.TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetMappings?.Invoke(viewManager, request!, metadata) ?? default;
    }
}