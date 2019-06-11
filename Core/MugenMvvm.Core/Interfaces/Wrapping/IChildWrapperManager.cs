using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IChildWrapperManager : IHasPriority
    {
        bool CanWrap(IWrapperManager wrapperManager, Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object? TryWrap(IWrapperManager wrapperManager, object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}