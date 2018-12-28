using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.ViewMapping
{
    public interface IViewMappingProvider
    {
        IEnumerable<IViewMappingItem> Mappings { get; }

        bool TryGetMappingsByView(Type viewType, out IReadOnlyCollection<IViewMappingItem>? mappings);

        bool TryGetMappingsByViewModel(Type viewModelType, out IReadOnlyCollection<IViewMappingItem>? mappings);

        bool TryGetMappingByViewModel(Type viewModelType, string? viewName, out IViewMappingItem? mapping);
    }
}