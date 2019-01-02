using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewMappingProvider
    {
        IEnumerable<IViewMappingInfo> Mappings { get; }

        bool TryGetMappingsByView(Type viewType, out IReadOnlyCollection<IViewMappingInfo>? mappings);

        bool TryGetMappingsByViewModel(Type viewModelType, out IReadOnlyCollection<IViewMappingInfo>? mappings);

        bool TryGetMappingByViewModel(Type viewModelType, string? viewName, out IViewMappingInfo? mapping);
    }
}