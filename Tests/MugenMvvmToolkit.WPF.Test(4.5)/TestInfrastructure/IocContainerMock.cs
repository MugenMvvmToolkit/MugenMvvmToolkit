using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Test.TestModels;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public class IocContainerMock : DisposableObject, IIocContainer
    {
        #region Fields

        private static int _countSt;
        private readonly int _count;

        #endregion

        #region ConstructorsS

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocContainerMock" /> class.
        /// </summary>
        public IocContainerMock(IocContainerMock iocContainerMock = null)
        {
            _count = Interlocked.Increment(ref _countSt);
            Parent = iocContainerMock;
            Container = this;
            if (iocContainerMock == null) return;
            GetFunc = iocContainerMock.GetFunc;
            BindToConstantFunc = iocContainerMock.BindToConstantFunc;
            BindFunc = iocContainerMock.BindFunc;
            UnbindFunc = iocContainerMock.UnbindFunc;
        }

        #endregion

        #region Properties

        public Func<Type, bool> CanResolveDelegate { get; set; }

        public Func<Type, string, IIocParameter[], object> GetFunc { get; set; }

        public Func<Type, string, IIocParameter[], object[]> GetAllFunc { get; set; }

        public Action<Type, object, string> BindToConstantFunc { get; set; }

        public Action<Type, Type, DependencyLifecycle, string> BindFunc { get; set; }

        public Action<Type> UnbindFunc { get; set; }

        public Func<IocContainerMock, IIocContainer> CreateChild { get; set; }

        #endregion

        #region Implementation of IIocContainer

        /// <summary>
        ///     Gets the id of <see cref="IIocContainer" />.
        /// </summary>
        int IIocContainer.Id
        {
            get { return _count; }
        }

        /// <summary>
        ///     Gets the parent ioc adapter.
        /// </summary>
        public IIocContainer Parent { get; private set; }

        /// <summary>
        ///     Gets the original ioc container.
        /// </summary>
        public object Container { get; private set; }

        /// <summary>
        ///     Creates a child ioc adapter.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IIocContainer" />.
        /// </returns>
        IIocContainer IIocContainer.CreateChild()
        {
            if (CreateChild == null)
                return new IocContainerMock(this)
                {
                    GetFunc = GetFunc,
                    BindFunc = BindFunc,
                    CanResolveDelegate = CanResolveDelegate,
                    BindToConstantFunc = BindToConstantFunc,
                    GetAllFunc = GetAllFunc,
                    UnbindFunc = UnbindFunc
                };
            return CreateChild(this);
        }

        /// <summary>
        ///     Gets an instance of the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        object IIocContainer.Get(Type service, string name, params IIocParameter[] parameters)
        {
            return GetFunc(service, name, parameters);
        }

        /// <summary>
        ///     Gets all instances of the specified service.
        /// </summary>
        /// <param name="service">Specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        IEnumerable<object> IIocContainer.GetAll(Type service, string name, params IIocParameter[] parameters)
        {
            return GetAllFunc(service, name, parameters);
        }

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        /// </summary>
        void IIocContainer.BindToConstant(Type service, object constValue, string name)
        {
            BindToConstantFunc(service, constValue, name);
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
        void IIocContainer.BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null)
        {
            throw new NotImplementedException();
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
        void IIocContainer.Bind(Type service, Type typeTo, DependencyLifecycle dependencyLifecycle, string name)
        {
            BindFunc(service, typeTo, dependencyLifecycle, name);
        }

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        void IIocContainer.Unbind(Type service)
        {
            UnbindFunc(service);
        }

        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <returns>
        ///     <c>True</c> if the specified service has been resolved; otherwise, <c>false</c>.
        /// </returns>
        bool IIocContainer.CanResolve(Type service, string name)
        {
            if (CanResolveDelegate == null)
                return false;
            return CanResolveDelegate(service);
        }

        #endregion

        #region Implementation of IServiceProvider

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
            return ((IIocContainer)this).Get(serviceType);
        }

        #endregion
    }
}