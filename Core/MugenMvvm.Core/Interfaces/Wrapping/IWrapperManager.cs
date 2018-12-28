using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager
    {
        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext context);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext context);
    }
}