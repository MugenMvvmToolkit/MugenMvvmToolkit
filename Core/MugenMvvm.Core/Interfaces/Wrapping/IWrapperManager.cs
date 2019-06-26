using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IComponentOwner<IWrapperManager>
    {
        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext? metadata = null);

        object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext? metadata = null);
    }
}