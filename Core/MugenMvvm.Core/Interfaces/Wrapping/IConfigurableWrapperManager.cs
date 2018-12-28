using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Wrapping
{
    public interface IConfigurableWrapperManager : IWrapperManager
    {
        void AddWrapper(Type wrapperType, Type implementation,
            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory = null);

        void AddWrapper<TWrapper>(Type implementation,
            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class;

        void AddWrapper<TWrapper, TImplementation>(Func<Type, IReadOnlyMetadataContext, bool>? condition = null,
            Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper;

        void Clear<TWrapper>();
    }
}