using System;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IWrapperManager : IComponentOwner<IWrapperManager>, IComponent<IMugenApplication>
    {
        bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext? metadata = null);

        object Wrap(object target, Type wrapperType, IReadOnlyMetadataContext? metadata = null);
    }
}