using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IHasListeners<IWrapperManagerListener>
    {
        IComponentCollection<IChildWrapperManager> Managers { get; }

        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata);
    }
}