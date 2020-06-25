using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewModelViewMappingProviderComponent : IComponent<IViewManager>
    {
        ItemOrList<IViewModelViewMapping, IReadOnlyList<IViewModelViewMapping>> TryGetMappings<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}