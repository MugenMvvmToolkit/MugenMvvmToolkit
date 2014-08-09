#region Copyright
// ****************************************************************************
// <copyright file="AutofacContainer.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the Autofac ioc adapter.
    /// </summary>
    public class AutofacContainer : DisposableObject, IIocContainer
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacContainer" /> class.
        /// </summary>
        public AutofacContainer()
            : this(new ContainerBuilder())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacContainer" /> class.
        /// </summary>
        public AutofacContainer(ContainerBuilder containerBuilder)
        {
            Should.NotBeNull(containerBuilder, "containerBuilder");
            containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource
            {
#if !WINDOWS_PHONE7
                RegistrationConfiguration = builder => builder.ExternallyOwned()
#endif
            });
            _container = containerBuilder.Build();
            _id = Interlocked.Increment(ref _idCounter);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacContainer" /> class.
        /// </summary>
        private AutofacContainer(ILifetimeScope container, IIocContainer parent)
        {
            Should.NotBeNull(container, "container");
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

        /// <summary>
        ///     true to throw an exception if the token is not valid;
        ///     Specifying false also suppresses some other exception conditions, but not all of them.
        /// </summary>
        public bool ThrowOnUnbind { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts parameters.
        /// </summary>
        protected Parameter[] ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return EmptyValue<Parameter>.ArrayInstance;

            var list = new List<Parameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter));
            list.Add(new ParameterContainer(parameters));
            return list.ToArrayFast();
        }

        /// <summary>
        ///     Converts parameter.
        /// </summary>
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

        /// <summary>
        ///     Sets the lifecycle.
        /// </summary>
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

        private static IList<IIocParameter> GetParameters(IEnumerable<Parameter> parameters)
        {
            if (parameters == null)
                return EmptyValue<IIocParameter>.ListInstance;
            var parameterContainer = parameters.OfType<ParameterContainer>().FirstOrDefault();
            if (parameterContainer == null)
                return EmptyValue<IIocParameter>.ListInstance;
            return parameterContainer.Parameters;
        }

        #endregion

        #region Implementation of IIocContainer

        /// <summary>
        ///     Gets the id of <see cref="IIocContainer" />.
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        ///     Gets the parent ioc adapter.
        /// </summary>
        public IIocContainer Parent
        {
            get { return _parent; }
        }

        /// <summary>
        ///     Gets the original ioc container.
        /// </summary>
        public object Container
        {
            get { return _container; }
        }

        /// <summary>
        ///     Creates a child ioc adapter.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IIocContainer" />.
        /// </returns>
        public IIocContainer CreateChild()
        {
            this.NotBeDisposed();
            return new AutofacContainer(_container.BeginLifetimeScope(), this);
        }

        /// <summary>
        ///     Gets an instance of the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        public object Get(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            if (name == null)
                return _container.Resolve(service, ConvertParameters(parameters));
            return _container.ResolveNamed(name, service, ConvertParameters(parameters));
        }

        /// <summary>
        ///     Gets all instances of the specified service.
        /// </summary>
        /// <param name="service">Specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        public IEnumerable<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            if (name == null)
                return (IEnumerable<object>)_container
                    .Resolve(typeof(IEnumerable<>).MakeGenericType(service), ConvertParameters(parameters));
            return (IEnumerable<object>)_container
                .ResolveNamed(name, typeof(IEnumerable<>).MakeGenericType(service), ConvertParameters(parameters));
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            var builder = new ContainerBuilder();
            if (name == null)
                builder.RegisterInstance(constValue).As(service);
            else
                builder.RegisterInstance(constValue).Named(name, service).SingleInstance();
            builder.Update(_container.ComponentRegistry);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified method.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="methodBindingDelegate">The specified factory delegate.</param>
        /// <param name="lifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="name">The specified binding name.</param>
        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            Should.NotBeNull(methodBindingDelegate, "methodBindingDelegate");
            var builder = new ContainerBuilder();
            IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> syntax = name == null
                ? builder.Register((context, args) => methodBindingDelegate(this, GetParameters(args))).As(service)
                : builder.Register((context, args) => methodBindingDelegate(this, GetParameters(args))).Named(name, service);
            SetLifetimeScope(lifecycle, syntax.RegistrationData);
            builder.Update(_container.ComponentRegistry);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified type.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="typeTo">The specified to type</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="dependencyLifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        public void Bind(Type service, Type typeTo, DependencyLifecycle dependencyLifecycle, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            Should.NotBeNull(typeTo, "typeTo");
            var builder = new ContainerBuilder();
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> syntax = name == null
                ? builder.RegisterType(typeTo).As(service)
                : builder.RegisterType(typeTo).Named(name, service);
            SetLifetimeScope(dependencyLifecycle, syntax.RegistrationData);
            builder.Update(_container.ComponentRegistry);
        }

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        public void Unbind(Type service)
        {
            Should.MethodBeSupported(!ThrowOnUnbind, "Unbind(Type service)");
            Tracer.Error("Unbind call on Autofac container type " + service);
        }

        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <returns>
        ///     <c>True</c> if the specified service has been resolved; otherwise, <c>false</c>.
        /// </returns>
        public bool CanResolve(Type service, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            if (name == null)
                return _container.IsRegistered(service);
            return _container.IsRegisteredWithName(name, service);
        }

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <returns>
        ///     A service object of type <paramref name="serviceType" />.
        ///     -or-
        ///     null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        /// <param name="serviceType">
        ///     An object that specifies the type of service object to get.
        /// </param>
        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose(bool disposing)
        {
            if (disposing)
                _container.Dispose();
            base.OnDispose(disposing);
        }

        #endregion
    }
}