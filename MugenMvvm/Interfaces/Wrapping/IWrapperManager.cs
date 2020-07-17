using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IComponentOwner<IWrapperManager>
    {
        bool CanWrap(Type wrapperType, object request, IReadOnlyMetadataContext? metadata = null);

        object? TryWrap(Type wrapperType, object request, IReadOnlyMetadataContext? metadata = null);
    }
}