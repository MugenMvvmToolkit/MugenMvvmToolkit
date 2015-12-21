#region Copyright

// ****************************************************************************
// <copyright file="AutofacContainer.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Features.ResolveAnything;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit
{
    public class AutofacContainer : IIocContainer
    {
        #region Nested types

        private sealed class ParameterContainer : Parameter
        {
            #region Fields

            private readonly IList<IIocParameter> _parameters;

            #endregion

            #region Constructors

            public ParameterContainer(IList<IIocParameter> parameters)
            {
                _parameters = parameters;
            }

            #endregion

            #region Properties

            public IList<IIocParameter> Parameters
            {
                get { return _parameters; }
            }

            #endregion

            #region Overrides of Parameter

            public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
            {
                valueProvider = null;
                return false;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static int _idCounter;
        private readonly ILifetimeScope _container;
        private readonly int _id;
        private readonly IIocContainer _parent;

        #endregion

        #region Constructor

        public AutofacContainer()
            : this(new ContainerBuilder())
        {
        }

        public AutofacContainer(ContainerBuilder containerBuilder)
        {
            Should.NotBeNull(containerBuilder, nameof(containerBuilder));
            containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource
            {
#if !WINDOWS_PHONE7
                RegistrationConfiguration = builder => builder.ExternallyOwned()
#endif
            });
            _container = containerBuilder.Build();
            _id = Interlocked.Increment(ref _idCounter);
        }

        private AutofacContainer(ILifetimeScope container, IIocContainer parent)
        {
            Should.NotBeNull(container, nameof(container));
            _container = container;
            _parent = parent;
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource
            {
#if !WINDOWS_PHONE7
                RegistrationConfiguration = builder => builder.ExternallyOwned()
#endif
            });
            containerBuilder.Update(container.ComponentRegistry);
            _id = Interlocked.Increment(ref _idCounter);
        }

        #endregion

        #region Properties

        public bool ThrowOnUnbind { get; set; }

        public ILifetimeScope Container
        {
            get { return _container; }
        }

        #endregion

        #region Methods

        protected IList<Parameter> ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return Empty.Array<Parameter>();

            var list = new List<Parameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter));
            list.Add(new ParameterContainer(parameters));
            return list;
        }

        protected virtual Parameter ConvertParameter(IIocParameter parameter)
        {
            if (parameter == null)
                return null;
            if (parameter.ParameterType == IocParameterType.Constructor)
                return new NamedParameter(parameter.Name, parameter.Value);
            if (parameter.ParameterType == IocParameterType.Property)
                return new NamedPropertyParameter(parameter.Name, parameter.Value);
            Tracer.Warn("The parameter with type {0} is not supported", parameter.ParameterType);
            return null;
        }

        protected virtual void SetLifetimeScope(DependencyLifecycle lifecycle, RegistrationData data)
        {
            if (lifecycle == DependencyLifecycle.SingleInstance)
            {
                data.Sharing = InstanceSharing.Shared;
                data.Lifetime = new RootScopeLifetime();
                return;
            }
            if (lifecycle == DependencyLifecycle.TransientInstance)
            {
                data.Sharing = InstanceSharing.None;
                data.Lifetime = new CurrentScopeLifetime();
                return;
            }
            Should.MethodBeSupported(false,
                "SetLifetimeScope(DependencyLifecycle dependencyLifecycle, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder)");
        }

        private static IList<IIocParameter> GetParameters(IIocParameter[] iocParameters, IEnumerable<Parameter> parameters)
        {
            if (parameters == null)
                return iocParameters;
            List<IIocParameter> result = null;
            foreach (var parameter in parameters.OfType<ParameterContainer>())
            {
                if (result == null)
                    result = new List<IIocParameter>();
                result.AddRange(parameter.Parameters);
            }
            if (result == null)
                return iocParameters;
            if (iocParameters != null)
                result.AddRange(iocParameters);
            return result;
        }

        #endregion

        #region Implementation of IIocContainer

        public int Id
        {
            get { return _id; }
        }

        public IIocContainer Parent
        {
            get { return _parent; }
        }

        object IIocContainer.Container
        {
            get { return _container; }
        }

        public IIocContainer CreateChild()
        {
            this.NotBeDisposed();
            return new AutofacContainer(_container.BeginLifetimeScope(), this) { ThrowOnUnbind = ThrowOnUnbind };
        }

        public object Get(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return _container.Resolve(service, ConvertParameters(parameters));
            return _container.ResolveNamed(name, service, ConvertParameters(parameters));
        }

        public IEnumerable<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return (IEnumerable<object>)_container
                    .Resolve(typeof(IEnumerable<>).MakeGenericType(service), ConvertParameters(parameters));
            return (IEnumerable<object>)_container
                .ResolveNamed(name, typeof(IEnumerable<>).MakeGenericType(service), ConvertParameters(parameters));
        }

        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var builder = new ContainerBuilder();
            if (name == null)
                builder.RegisterInstance(constValue).As(service);
            else
                builder.RegisterInstance(constValue).Named(name, service).SingleInstance();
            builder.Update(_container.ComponentRegistry);
        }

        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            if (parameters == null)
                parameters = Empty.Array<IIocParameter>();
            var builder = new ContainerBuilder();
            IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> syntax = name == null
                ? builder.Register((context, args) => methodBindingDelegate(this, GetParameters(parameters, args))).As(service)
                : builder.Register((context, args) => methodBindingDelegate(this, GetParameters(parameters, args))).Named(name, service);
            SetLifetimeScope(lifecycle, syntax.RegistrationData);
            builder.Update(_container.ComponentRegistry);
        }

        public void Bind(Type service, Type typeTo, DependencyLifecycle dependencyLifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            var builder = new ContainerBuilder();
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> syntax = name == null
                ? builder.RegisterType(typeTo).As(service)
                : builder.RegisterType(typeTo).Named(name, service);
            if (parameters != null)
            {
                for (int index = 0; index < parameters.Length; index++)
                {
                    var iocParameter = parameters[index];
                    var parameter = ConvertParameter(iocParameter);
                    if (iocParameter.ParameterType == IocParameterType.Property)
                        syntax.WithProperty(parameter);
                    else
                        syntax.WithParameter(parameter);
                }
            }
            SetLifetimeScope(dependencyLifecycle, syntax.RegistrationData);
            builder.Update(_container.ComponentRegistry);
        }

        public void Unbind(Type service)
        {
            Should.MethodBeSupported(!ThrowOnUnbind, "Unbind(Type service)");
            Tracer.Error("Unbind call on Autofac container type " + service);
        }

        public bool CanResolve(Type service, string name = null)
        {
            Should.NotBeNull(service, nameof(service));
            if (IsDisposed)
                return false;
            if (name == null)
                return _container.IsRegistered(service);
            return _container.IsRegisteredWithName(name, service);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            _container.Dispose();
            var handler = Disposed;
            if (handler != null)
            {
                Disposed = null;
                handler(this, EventArgs.Empty);
            }
        }

        public bool IsDisposed { get; private set; }

        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        #endregion
    }
}
