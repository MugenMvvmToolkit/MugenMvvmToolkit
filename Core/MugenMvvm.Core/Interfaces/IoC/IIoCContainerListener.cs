using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIoCContainerListener : IListener
    {
        void OnBindToConstant(IIoCContainer container, Type service, object? instance, IReadOnlyMetadataContext metadata);

        void OnBindToType(IIoCContainer container, Type service, Type typeTo, IoCDependencyLifecycle lifecycle, IReadOnlyMetadataContext metadata);

        void OnBindToMethod(IIoCContainer container, Type service, Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IoCDependencyLifecycle lifecycle, IReadOnlyMetadataContext metadata);

        void OnUnbind(IIoCContainer container, Type service, IReadOnlyMetadataContext metadata);

        void OnActivated(IIoCContainer container, Type service, object? instance, IReadOnlyMetadataContext metadata);

        void OnChildContainerCreated(IIoCContainer container, IIoCContainer childContainer, IReadOnlyMetadataContext metadata);

        void OnDisposed(IIoCContainer container);
    }
}