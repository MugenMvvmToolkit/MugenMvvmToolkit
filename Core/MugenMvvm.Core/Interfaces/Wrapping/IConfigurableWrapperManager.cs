using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IConfigurableWrapperManager : IWrapperManager//todo merge, add factory interface
    {
        void AddWrapper(Type wrapperType, Type implementation,
            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory = null);//todo register, fix change implementation

        void RemoveWrapper(Type wrapperType);
    }
}