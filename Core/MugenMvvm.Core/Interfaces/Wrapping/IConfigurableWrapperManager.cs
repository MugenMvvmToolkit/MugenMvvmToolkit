using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IConfigurableWrapperManager : IWrapperManager
    {
        void AddWrapper(Type wrapperType, Type implementation,
            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory = null);

        void Clear<TWrapper>();
    }
}