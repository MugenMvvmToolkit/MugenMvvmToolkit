using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIocContainer : IHasListeners<IIocContainerListener>, IDisposable, IServiceProvider
    {
        int Id { get; }

        IIocContainer? Parent { get; }

        object Container { get; }

        IIocContainer CreateChild(IReadOnlyMetadataContext? metadata = null);

        bool CanResolve(Type service, IReadOnlyMetadataContext? metadata = null);

        object Get(Type service, IReadOnlyMetadataContext? metadata = null);

        IEnumerable<object> GetAll(Type service, IReadOnlyMetadataContext? metadata = null);

        void BindToConstant(Type service, object? instance, IReadOnlyMetadataContext? metadata = null);

        void BindToType(Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null);

        void BindToMethod(Type service, IocBindingDelegate bindingDelegate, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null);

        void Unbind(Type service, IReadOnlyMetadataContext? metadata = null);
    }
}