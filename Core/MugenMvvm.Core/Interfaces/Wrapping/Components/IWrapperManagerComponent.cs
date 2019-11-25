using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping.Components
{
    public interface IWrapperManagerComponent : IComponent<IWrapperManager>
    {
        bool CanWrap(IWrapperManager wrapperManager, Type targetType, Type wrapperType, IReadOnlyMetadataContext? metadata);

        object? TryWrap(IWrapperManager wrapperManager, object target, Type wrapperType, IReadOnlyMetadataContext? metadata);
    }
}