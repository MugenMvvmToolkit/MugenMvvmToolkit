using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManagerListener : IComponent<IWrapperManager>
    {
        void OnWrapped(IWrapperManager wrapperManager, object wrapper, object item, Type wrapperType, IReadOnlyMetadataContext? metadata);
    }
}