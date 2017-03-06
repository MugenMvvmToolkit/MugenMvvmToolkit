#region Copyright

// ****************************************************************************
// <copyright file="NinjectContainer.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

            public IList<IIocParameter> Parameters => _parameters;

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

            public string Name => "~@~params";

            public bool ShouldInherit => true;

            #endregion
        }

        #endregion

        #region Fields

        private static int _idCounter;
        private readonly IKernel _kernel;
        private readonly IIocContainer _parent;

        #endregion

        #region Properties

        public IKernel Container => _kernel;

        #endregion

        #region Constructor

        public NinjectContainer()
            : this(new StandardKernel())
        {
        }

        public NinjectContainer(IKernel kernel, IIocContainer parent = null)
        {
            Should.NotBeNull(kernel, nameof(kernel));
            _kernel = kernel;
            _parent = parent;
            Id = Interlocked.Increment(ref _idCounter);
        }

        #endregion

        #region Methods

        protected IParameter[] ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return Empty.Array<IParameter>();

            var list = new List<IParameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter, false));
            list.Add(new ParameterContainer(parameters));
            return list.ToArrayEx();
        }

        protected virtual IParameter ConvertParameter(IIocParameter parameter, bool isDelegate)
        {
            if (parameter == null)
                return null;
            if (parameter.ParameterType == IocParameterType.Constructor)
            {
                if (isDelegate)
                    return new ConstructorArgument(parameter.Name, context => parameter.Value);
                return new ConstructorArgument(parameter.Name, parameter.Value);
            }
            if (parameter.ParameterType == IocParameterType.Property)
            {
                if (isDelegate)
                    return new PropertyValue(parameter.Name, context => parameter.Value);
                return new PropertyValue(parameter.Name, parameter.Value);
            }
            Tracer.Warn("The parameter with type {0} is not supported", parameter.ParameterType);
            return null;
        }

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

        private static IList<IIocParameter> GetParameters(IIocParameter[] iocParameters, IContext context)
        {
            if (context.Parameters == null || context.Parameters.Count == 0)
                return iocParameters;
            List<IIocParameter> result = null;
            foreach (var parameter in context.Parameters.OfType<ParameterContainer>())
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

        public int Id { get; }

        public bool IsDisposed => _kernel.IsDisposed;

        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        public IIocContainer Parent => _parent;

        object IIocContainer.Container => _kernel;

        public IIocContainer CreateChild()
        {
            this.NotBeDisposed();
            return new NinjectContainer(new ChildKernel(_kernel), this);
        }

        public object Get(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return _kernel.Get(service, ConvertParameters(parameters));
            return _kernel.Get(service, name, ConvertParameters(parameters));
        }

        public IEnumerable<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return _kernel.GetAll(service, ConvertParameters(parameters));
            return _kernel.GetAll(service, name, ConvertParameters(parameters));
        }

        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel.Bind(service).ToConstant(constValue);
            if (name != null)
                syntax.Named(name);
        }

        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate,
            DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            if (parameters == null)
                parameters = Empty.Array<IIocParameter>();
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel
                .Bind(service)
                .ToMethod(context => methodBindingDelegate(this, GetParameters(parameters, context)));
            SetLifecycle(syntax, lifecycle);
            if (name != null)
                syntax.Named(name);
        }

        public void Bind(Type service, Type typeTo, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel
                .Bind(service)
                .To(typeTo);
            if (parameters != null)
            {
                for (int index = 0; index < parameters.Length; index++)
                    syntax.WithParameter(ConvertParameter(parameters[index], true));
            }
            SetLifecycle(syntax, lifecycle);
            if (name != null)
                syntax.Named(name);
        }

        public void Unbind(Type service)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            _kernel.Unbind(service);
        }

        public bool CanResolve(Type service, string name = null)
        {
            Should.NotBeNull(service, nameof(service));
            if (IsDisposed)
                return false;
            Func<IBindingMetadata, bool> canResolve = null;
            if (name != null)
                canResolve = metadata => metadata.Name == name;
            IRequest req = _kernel.CreateRequest(service, canResolve, Empty.Array<IParameter>(), false, true);
            return _kernel.CanResolve(req, true);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            _kernel.Dispose();
            var handler = Disposed;
            if (handler != null)
            {
                Disposed = null;
                handler(this, EventArgs.Empty);
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return _kernel.GetService(serviceType);
        }

        #endregion
    }
}
