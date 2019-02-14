using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManagerListener
    {
        void OnWrapped(IWrapperManager wrapperManager, object item, Type wrapperType, object wrapper, IReadOnlyMetadataContext metadata);
    }
}