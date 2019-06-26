using System;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIocContainerListener : IComponent<IIocContainer>
    {
        void OnBindToConstant(IIocContainer container, Type service, object? instance, IReadOnlyMetadataContext? metadata);

        void OnBindToType(IIocContainer container, Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata);

        void OnBindToMethod(IIocContainer container, Type service, IocBindingDelegate bindingDelegate, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata);

        void OnUnbind(IIocContainer container, Type service, IReadOnlyMetadataContext? metadata);

        void OnActivated(IIocContainer container, Type service, object? member, object? instance, IReadOnlyMetadataContext? bindingMetadata, IReadOnlyMetadataContext? metadata);

        void OnChildContainerCreated(IIocContainer container, IIocContainer childContainer, IReadOnlyMetadataContext? metadata);

        void OnDisposed(IIocContainer container);
    }
}