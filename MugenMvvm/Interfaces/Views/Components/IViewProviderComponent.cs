using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewProviderComponent : IComponent<IViewManager>
    {
        ItemOrList<IView, IReadOnlyList<IView>> TryGetViews<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}