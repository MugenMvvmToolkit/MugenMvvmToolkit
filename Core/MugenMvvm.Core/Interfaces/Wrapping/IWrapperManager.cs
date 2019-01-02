using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager
    {
        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}