#region Copyright

// ****************************************************************************
// <copyright file="NinjectContainer.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.ChildKernel;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Planning.Targets;
using Ninject.Syntax;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the Ninject ioc adapter.
    /// </summary>
    public class NinjectContainer : IIocContainer
    {
        #region Nested types

        private sealed class ParameterContainer : IParameter
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

            #region Implementation of IParameter

            public bool Equals(IParameter other)
            {
                return ReferenceEquals(this, other);
            }

            public object GetValue(IContext context, ITarget target)
            {
                return _parameters;
            }

            public string Name
            {
                get { return "~@~params"; }
            }

            public bool ShouldInherit
            {
                get { return true; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static int _idCounter;
        private readonly int _id;
        private readonly IKernel _kernel;
        private readonly IIocContainer _parent;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original ioc container.
        /// </summary>
        public IKernel Container
        {
            get { return _kernel; }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="NinjectContainer" /> class.
        /// </summary>
        public NinjectContainer()
            : this(new StandardKernel())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NinjectContainer" /> class.
        /// </summary>
        public NinjectContainer(IKernel kernel, IIocContainer parent = null)
        {
            Should.NotBeNull(kernel, "kernel");
            _kernel = kernel;
            _parent = parent;
            _id = Interlocked.Increment(ref _idCounter);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts parameters.
        /// </summary>
        protected IParameter[] ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return Empty.Array<IParameter>();

            var list = new List<IParameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter));
            list.Add(new ParameterContainer(parameters));
            return list.ToArrayEx();
        }

        /// <summary>
        ///     Converts parameter.
        /// </summary>
        protected virtual IParameter ConvertParameter(IIocParameter parameter)
        {
            if (parameter == null)
                return null;
            if (parameter.ParameterType == IocParameterType.Constructor)
                return new ConstructorArgument(parameter.Name, parameter.Value);
            if (parameter.ParameterType == IocParameterType.Property)
                return new PropertyValue(parameter.Name, parameter.Value);
            Tracer.Warn("The parameter with type {0} is not supported", parameter.ParameterType);
            return null;
        }

        /// <summary>
        ///     Sets the lifecycle.
        /// </summary>
        protected virtual void SetLifecycle(IBindingInSyntax<object> syntax, DependencyLifecycle lifecycle)
        {
            if (lifecycle == DependencyLifecycle.SingleInstance)
            {
                syntax.InSingletonScope();
                return;
            }
            if (lifecycle == DependencyLifecycle.TransientInstance)
            {
                syntax.InTransientScope();
                return;
            }
            Should.MethodBeSupported(false,
                "SetLifecycle(IBindingInSyntax<object> syntax, DependencyLifecycle lifecycle)");
        }

        private static IList<IIocParameter> GetParameters(IContext context)
        {
            if (context.Parameters == null || context.Parameters.Count == 0)
                return Empty.Array<IIocParameter>();
            var parameterContainer = context.Parameters.OfType<ParameterContainer>().FirstOrDefault();
            if (parameterContainer == null)
                return Empty.Array<IIocParameter>();
            return parameterContainer.Parameters;
        }

        private void OnDisposed()
        {
            var handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
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
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _kernel.IsDisposed; }
        }

        /// <summary>
        ///     Occured after disposed current <see cref="IDisposableObject" />.
        /// </summary>
        public event EventHandler<IDisposableObject, EventArgs> Disposed;

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
        object IIocContainer.Container
        {
            get { return _kernel; }
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
            return new NinjectContainer(new ChildKernel(_kernel), this);
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
                return _kernel.Get(service, ConvertParameters(parameters));
            return _kernel.Get(service, name, ConvertParameters(parameters));
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
                return _kernel.GetAll(service, ConvertParameters(parameters));
            return _kernel.GetAll(service, name, ConvertParameters(parameters));
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel.Bind(service).ToConstant(constValue);
            if (name != null)
                syntax.Named(name);
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
        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate,
            DependencyLifecycle lifecycle, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            Should.NotBeNull(methodBindingDelegate, "methodBindingDelegate");
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel
                .Bind(service)
                .ToMethod(context => methodBindingDelegate(this, GetParameters(context)));
            SetLifecycle(syntax, lifecycle);
            if (name != null)
                syntax.Named(name);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified type.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="typeTo">The specified to type</param>
        /// <param name="lifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="name">The specified binding name.</param>
        public void Bind(Type service, Type typeTo, DependencyLifecycle lifecycle, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            Should.NotBeNull(typeTo, "typeTo");
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel
                .Bind(service)
                .To(typeTo);
            SetLifecycle(syntax, lifecycle);
            if (name != null)
                syntax.Named(name);
        }

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        public void Unbind(Type service)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            _kernel.Unbind(service);
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
            Should.NotBeNull(service, "service");
            if (IsDisposed)
                return false;
            Func<IBindingMetadata, bool> canResolve = null;
            if (name != null)
                canResolve = metadata => metadata.Name == name;
            IRequest req = _kernel.CreateRequest(service, canResolve, Empty.Array<IParameter>(), false, true);
            return _kernel.CanResolve(req, true);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            lock (_kernel)
            {
                if (IsDisposed)
                    return;
                _kernel.Dispose();
                OnDisposed();
            }
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
            return _kernel.GetService(serviceType);
        }

        #endregion
    }
}