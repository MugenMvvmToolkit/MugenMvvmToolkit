#region Copyright

// ****************************************************************************
// <copyright file="IocContainerMock.cs">
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

        int IIocContainer.Id => _count;

        public IIocContainer Parent { get; private set; }

        public object Container { get; private set; }

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

        object IIocContainer.Get(Type service, string name, params IIocParameter[] parameters)
        {
            return GetFunc(service, name, parameters);
        }

        IEnumerable<object> IIocContainer.GetAll(Type service, string name, params IIocParameter[] parameters)
        {
            return GetAllFunc(service, name, parameters);
        }

        void IIocContainer.BindToConstant(Type service, object constValue, string name)
        {
            BindToConstantFunc(service, constValue, name);
        }

        void IIocContainer.BindToMethod(Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        void IIocContainer.Bind(Type service, Type typeTo, DependencyLifecycle dependencyLifecycle, string name, params IIocParameter[] parameters)
        {
            BindFunc(service, typeTo, dependencyLifecycle, name);
        }

        void IIocContainer.Unbind(Type service)
        {
            UnbindFunc(service);
        }

        bool IIocContainer.CanResolve(Type service, string name)
        {
            if (CanResolveDelegate == null)
                return false;
            return CanResolveDelegate(service);
        }

        #endregion

        #region Implementation of IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IIocContainer)this).Get(serviceType);
        }

        #endregion
    }
}
