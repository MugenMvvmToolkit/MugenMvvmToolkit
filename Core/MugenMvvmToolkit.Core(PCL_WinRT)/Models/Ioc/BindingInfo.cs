#region Copyright

// ****************************************************************************
// <copyright file="BindingInfo.cs">
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
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Models.IoC
{
    [StructLayout(LayoutKind.Auto)]
    public struct BindingInfo<T>
    {
        #region Fields

        public static readonly BindingInfo<T> Empty;

        public readonly T Instance;
        public readonly DependencyLifecycle Lifecycle;
        public readonly Func<IIocContainer, IList<IIocParameter>, T> MethodBindingDelegate;
        public readonly string Name;
        public readonly Type Type;
        public readonly IIocParameter[] Parameters;

        private readonly bool _notEmpty;

        #endregion

        #region Constructors

        static BindingInfo()
        {
            Empty = default(BindingInfo<T>);
        }

        private BindingInfo(Type type, Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate, T instance,
            DependencyLifecycle lifecycle, string name, IIocParameter[] parameters)
        {
            Type = type;
            MethodBindingDelegate = methodBindingDelegate;
            Instance = instance;
            Lifecycle = lifecycle;
            Name = name;
            Parameters = parameters ?? Empty.Parameters;
            _notEmpty = true;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return !_notEmpty; }
        }

        #endregion

        #region Methods

        public static BindingInfo<T> FromInstance(T instance, string name = null)
        {
            Should.NotBeNull(instance, nameof(instance));
            return new BindingInfo<T>(null, null, instance, DependencyLifecycle.SingleInstance, name, null);
        }

        public static BindingInfo<T> FromType<TService>(DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
            where TService : T
        {
            return new BindingInfo<T>(typeof(TService), null, default(T), lifecycle, name, parameters);
        }

        public static BindingInfo<T> FromType(Type serviceType, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            Should.NotBeNull(serviceType, nameof(serviceType));
            Should.BeOfType<T>(serviceType, "serviceType");
            return new BindingInfo<T>(serviceType, null, default(T), lifecycle, name, parameters);
        }

        public static BindingInfo<T> FromMethod(Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate,
            DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
        {
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            return new BindingInfo<T>(null, methodBindingDelegate, default(T), lifecycle, name, parameters);
        }

        public void SetBinding(IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            if (IsEmpty)
                return;
            if (MethodBindingDelegate != null)
                iocContainer.BindToMethod(MethodBindingDelegate, Lifecycle, Name, Parameters);
            else if (Type != null)
                iocContainer.Bind(typeof(T), Type, Lifecycle, Name, Parameters);
            else
                iocContainer.BindToConstant(typeof(T), Instance, Name);
        }

        #endregion
    }
}
