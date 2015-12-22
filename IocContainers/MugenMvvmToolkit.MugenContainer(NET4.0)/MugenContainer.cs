#region Copyright

// ****************************************************************************
// <copyright file="MugenContainer.cs">
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

            public IList<IIocParameter> Parameters => _parameters;

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

        public MugenContainer()
            : this(new MugenInjector(new DefaultInjectorSetting
            {
#if NET4
                IsAutoScanAssembly = false
#endif
            }))
        {
        }

        public MugenContainer(IInjector injector, IIocContainer parent = null)
        {
            Should.NotBeNull(injector, nameof(injector));
            _injector = injector;
            _injector.Disposed += InjectorOnDisposed;
            _parent = parent;
            _id = Interlocked.Increment(ref _idCounter);
        }

        #endregion

        #region Properties

        public IInjector Container => _injector;

        #endregion

        #region Methods

        protected IInjectionParameter[] ConvertParameters(IList<IIocParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return Empty.Array<IInjectionParameter>();

            var list = new List<IInjectionParameter>();
            foreach (var iocParameter in parameters)
                list.AddIfNotNull(ConvertParameter(iocParameter, false));
            list.Add(new ParameterContainer(parameters));
            return list.ToArrayEx();
        }

        protected virtual IInjectionParameter ConvertParameter(IIocParameter parameter, bool isDelegate)
        {
            if (parameter == null)
                return null;
            if (parameter.ParameterType == IocParameterType.Constructor)
            {
                if (isDelegate)
                    return new ConstructorParameter(parameter.Name, context => parameter.Value);
                return new ConstructorParameter(parameter.Name, parameter.Value);
            }
            if (parameter.ParameterType == IocParameterType.Property)
            {
                if (isDelegate)
                    return new PropertyParameter(parameter.Name, context => parameter.Value);
                return new PropertyParameter(parameter.Name, parameter.Value);
            }
            Tracer.Warn("The parameter with type {0} is not supported", parameter.ParameterType);
            return null;
        }

        protected virtual IScopeLifecycle GetScope(DependencyLifecycle lifecycle)
        {
            if (lifecycle == DependencyLifecycle.SingleInstance)
                return new SingletonScopeLifecycle();
            if (lifecycle == DependencyLifecycle.TransientInstance)
                return new TransientScopeLifecycle();
            Should.MethodBeSupported(false, "GetScope(DependencyLifecycle lifecycle)");
            return null;
        }

        private static IList<IIocParameter> GetParameters(IIocParameter[] iocParameters, IBindingContext context)
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

        public int Id => _id;

        public bool IsDisposed => _injector.IsDisposed;

        public event EventHandler<Interfaces.Models.IDisposableObject, EventArgs> Disposed;

        public IIocContainer Parent => _parent;

        object IIocContainer.Container => _injector;

        public IIocContainer CreateChild()
        {
            this.NotBeDisposed();
            return new MugenContainer(_injector.CreateChild(), this);
        }

        public object Get(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return _injector.Get(service, ConvertParameters(parameters));
            return _injector.Get(service, name, null, ConvertParameters(parameters));
        }

        public IEnumerable<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (name == null)
                return _injector.GetAll(service, ConvertParameters(parameters));
            return _injector.GetAll(service, name, null, ConvertParameters(parameters));
        }

        public void BindToConstant(Type service, object constValue, string name = null)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            IConstantObjectPriorityWhenSyntax syntax = _injector.BindWithManualBuild(service).ToConstant(constValue);
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
        }

        public void BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            if (parameters == null)
                parameters = Empty.Array<IIocParameter>();
            IMethodCallbackObjectPriorityUseWithSyntax syntax = _injector.BindWithManualBuild(service).ToMethod(context => methodBindingDelegate(this, GetParameters(parameters, context))).InScope(GetScope(lifecycle));
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
        }

        public void Bind(Type service, Type typeTo, DependencyLifecycle dependencyLifecycle, string name = null, params IIocParameter[] parameters)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            ITypeCallbackConstructorObjectPriorityUseWithSyntax syntax = _injector.BindWithManualBuild(service).To(typeTo).InScope(GetScope(dependencyLifecycle));
            if (parameters != null)
            {
                for (int index = 0; index < parameters.Length; index++)
                    syntax.WithParameter(ConvertParameter(parameters[index], true));
            }
            if (name != null)
                syntax.NamedBinding(name);
            syntax.Build();
        }

        public void Unbind(Type service)
        {
            this.NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            _injector.Unbind(service);
        }

        public bool CanResolve(Type service, string name = null)
        {
            Should.NotBeNull(service, nameof(service));
            if (IsDisposed)
                return false;
            if (name == null)
                return _injector.CanResolve(service, true, false);
            return _injector.CanResolve(service, name, true, false);
        }

        public void Dispose()
        {
            _injector.Dispose();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return _injector.GetService(serviceType);
        }

        #endregion
    }
}
