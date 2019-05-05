using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIocContainerListener : IListener
    {
        void OnBindToConstant(IIocContainer container, Type service, object? instance, IReadOnlyMetadataContext metadata);

        void OnBindToType(IIocContainer container, Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext metadata);

        void OnBindToMethod(IIocContainer container, Type service, Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext metadata);

        void OnUnbind(IIocContainer container, Type service, IReadOnlyMetadataContext metadata);

        void OnActivated(IIocContainer container, Type service, object? instance, IReadOnlyMetadataContext metadata);

        void OnChildContainerCreated(IIocContainer container, IIocContainer childContainer, IReadOnlyMetadataContext metadata);

        void OnDisposed(IIocContainer container);
    }
}