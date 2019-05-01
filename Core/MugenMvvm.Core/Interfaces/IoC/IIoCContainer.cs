using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIoCContainer : IHasListeners<IIoCContainerListener>, IDisposable, IServiceProvider
    {
        int Id { get; }

        IIoCContainer? Parent { get; }

        object Container { get; }

        IIoCContainer CreateChild(IReadOnlyMetadataContext? metadata = null);

        bool CanResolve(Type service, IReadOnlyMetadataContext? metadata = null);

        object Get(Type service, IReadOnlyMetadataContext? metadata = null);

        IEnumerable<object> GetAll(Type service, IReadOnlyMetadataContext? metadata = null);

        void BindToConstant(Type service, object? instance, IReadOnlyMetadataContext? metadata = null);

        void BindToType(Type service, Type typeTo, IoCDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null);

        void BindToMethod(Type service, Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IoCDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null);

        void Unbind(Type service, IReadOnlyMetadataContext? metadata = null);
    }
}