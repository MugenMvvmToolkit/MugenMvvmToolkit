using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IHasListeners<IWrapperManagerListener>
    {
        void AddWrapperFactory(IWrapperManagerFactory factory);

        void RemoveWrapperFactory(IWrapperManagerFactory factory);

        IReadOnlyList<IWrapperManagerFactory> GetWrapperFactories();

        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}