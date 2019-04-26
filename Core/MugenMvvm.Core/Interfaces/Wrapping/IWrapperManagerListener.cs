using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManagerListener : IListener
    {
        void OnWrapped(IWrapperManager wrapperManager, object wrapper, object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}