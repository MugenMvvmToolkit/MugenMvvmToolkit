#region Copyright
// ****************************************************************************
// <copyright file="MugenContainer.cs">
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
using MugenInjection;
using MugenInjection.Core;
using MugenInjection.Interface;
using MugenInjection.Parameters;
using MugenInjection.Scope;
using MugenInjection.Syntax.Constant;
using MugenInjection.Syntax.Method;
using MugenInjection.Syntax.Type;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the MugenInjection ioc adapter.
    /// </summary>
    public class MugenContainer : IIocContainer
    {
        #region Nested types

        private sealed class ParameterContainer : InjectionParameter
        {
            #region Fields

            private readonly IList<IIocParameter> _parameters;

            #endregion

            #region Constructors

            public ParameterContainer(IList<IIocParameter> parameters)
                : base(MemberTypes.TypeInfo, "``3params", GeValueEmpty)
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

            #region Methods

            private static object GeValueEmpty(IParameterContext parameterContext)
            {
                return null;
            }

            #endregion

        }

        #endregion

        #region Fields

        private static int _idCounter;
        private readonly int _id;
        private readonly IInjector _injector;
        private readonly IIocContainer _parent;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="MugenContainer" /> class.
        /// </summary>
        public MugenContainer()
            : this(new MugenInjector(new DefaultInjectorSetting
            {
#if NET4                
                IsAutoScanAssembly = false
#endif
            }))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MugenContainer" /> class.
        /// </summary>
        public MugenContainer(IInjector injector, IIocContainer parent = null)
        {
            Should.NotBeNull(injector, "injector");
            _injector = injector;
            _injector.Disposed += InjectorOnDisposed;
            _parent = parent;
            _id = Interlocked.Increment(ref _idCounter);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original ioc container.
        /// </summary>
        public IInjector Container
        {
            get { return _injector; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts parameters.
        /// </summary>
        protected IInjectionParameter[] ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return Empty.Array<IInjectionParameter>();

            var list = new List<IInjectionParameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter));
            list.Add(new ParameterContainer(parameters));
            return list.ToArrayEx();
        }

        /// <summary>
        ///     Converts parameter.
        /// </summary>
        protected virtual IInjectionParameter ConvertParameter(IIocParameter parameter)
        {
            if (parameter == null)
                return null;
            if (parameter.ParameterType == IocParameterType.Constructor)
                return new ConstructorParameter(parameter.Name, parameter.Value);
            if (parameter.ParameterType == IocParameterType.Property)
                return new PropertyParameter(parameter.Name, parameter.Value);
            Tracer.Warn("The parameter with type {0} is not supported", parameter.ParameterType);
            return null;
        }

        /// <summary>
        ///     Gets the scope lifecycle.
        /// </summary>
        protected virtual IScopeLifecycle GetScope(DependencyLifecycle lifecycle)
        {
            if (lifecycle == DependencyLifecycle.SingleInstance)
                return new SingletonScopeLifecycle();
            if (lifecycle == DependencyLifecycle.TransientInstance)
                return new TransientScopeLifecycle();
            Should.MethodBeSupported(false, "GetScope(DependencyLifecycle lifecycle)");
            return null;
        }

        private static IList<IIocParameter> GetParameters(IBindingContext context)
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

        private void InjectorOnDisposed(IDisposableObject disposableObject)
        {
            disposableObject.Disposed -= InjectorOnDisposed;
            OnDisposed();
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
            get { return _injector.IsDisposed; }
        }

        /// <summary>
        ///     Occured after disposed current <see cref="Interfaces.Models.IDisposableObject" />.
        /// </summary>
        public event EventHandler<Interfaces.Models.IDisposableObject, EventArgs> Disposed;

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
            get { return _injector; }
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
            return new MugenContainer(_injector.CreateChild(), this);
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
                return _injector.Get(service, ConvertParameters(parameters));
            return _injector.Get(service, name, null, ConvertParameters(parameters));
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
                return _injector.GetAll(service, ConvertParameters(parameters));
            return _injector.GetAll(service, name, null, ConvertParameters(parameters));
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            IConstantObjectPriorityWhenSyntax syntax = _injector
                .BindWithManualBuild(service)
                .ToConstant(constValue);
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
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
            IMethodCallbackObjectPriorityUseWithSyntax syntax = _injector
                .BindWithManualBuild(service)
                .ToMethod(context => methodBindingDelegate(this, GetParameters(context)))
                .InScope(GetScope(lifecycle));
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
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
            ITypeCallbackConstructorObjectPriorityUseWithSyntax syntax = _injector
                .BindWithManualBuild(service)
                .To(typeTo)
                .InScope(GetScope(dependencyLifecycle));
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
        }

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        public void Unbind(Type service)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, "service");
            _injector.Unbind(service);
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
                return _injector.CanResolve(service, true, false);
            return _injector.CanResolve(service, name, true, false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _injector.Dispose();
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
            return _injector.GetService(serviceType);
        }

        #endregion
    }
}