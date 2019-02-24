using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManagerListener : IListener
    {
        void OnWrapped(IWrapperManager wrapperManager, object item, Type wrapperType, object wrapper, IReadOnlyMetadataContext metadata);
    }
}