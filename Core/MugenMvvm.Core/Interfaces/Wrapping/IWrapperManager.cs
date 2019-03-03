using System;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IHasListeners<IWrapperManagerListener>
    {
        IComponentCollection<IWrapperManagerFactory> WrapperFactories { get; }

        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}