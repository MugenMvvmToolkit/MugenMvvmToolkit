using System;
using System.Collections.Generic;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IIoCContainer : IDisposable, IServiceProviderEx
    {
        int Id { get; }

        IIoCContainer? Parent { get; }

        object Container { get; }

        IIoCContainer CreateChild();

        bool CanResolve(Type service, string? name = null);

        object Get(Type service, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null);

        IEnumerable<object> GetAll(Type service, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null);

        void BindToType(Type service, Type typeTo, IoCDependencyLifecycle lifecycle, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null);

        void BindToConstant(Type service, object instance, string? name = null);

        void BindToMethod(Type service, Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, object> methodBindingDelegate, IoCDependencyLifecycle lifecycle,
            string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null);

        void Unbind(Type service);
    }
}