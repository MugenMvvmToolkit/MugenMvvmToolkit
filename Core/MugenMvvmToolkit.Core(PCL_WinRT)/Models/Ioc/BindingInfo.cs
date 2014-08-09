#region Copyright
// ****************************************************************************
// <copyright file="BindingInfo.cs">
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
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Models.IoC
{
    /// <summary>
    ///     Represents an information about binding.
    /// </summary>
    public struct BindingInfo<T>
    {
        #region Fields

        /// <summary>
        ///     Gets the empty instance of BindingInfo.
        /// </summary>
        public static readonly BindingInfo<T> Empty = default(BindingInfo<T>);

        /// <summary>
        ///     Gets the constant instance.
        /// </summary>
        public readonly T Instance;

        /// <summary>
        ///     Gets the dependecy lifecycle of service.
        /// </summary>
        public readonly DependencyLifecycle Lifecycle;

        /// <summary>
        ///     Gets the method binding delegate.
        /// </summary>
        public readonly Func<IIocContainer, IList<IIocParameter>, T> MethodBindingDelegate;

        /// <summary>
        ///     Gets the name of binding.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     Gets the type of service.
        /// </summary>
        public readonly Type Type;

        private readonly bool _notEmpty;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingInfo{T}" /> class.
        /// </summary>
        private BindingInfo(Type type, Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate, T instance,
            DependencyLifecycle lifecycle, string name)
        {
            Type = type;
            MethodBindingDelegate = methodBindingDelegate;
            Instance = instance;
            Lifecycle = lifecycle;
            Name = name;
            _notEmpty = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value that indicates that binding info is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return !_notEmpty; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of BindingInfo from an instance of service.
        /// </summary>
        public static BindingInfo<T> FromInstance(T instance, string name = null)
        {
            Should.NotBeNull(instance, "instance");
            return new BindingInfo<T>(null, null, instance, DependencyLifecycle.SingleInstance, name);
        }

        /// <summary>
        ///     Creates an instance of BindingInfo from a type.
        /// </summary>
        public static BindingInfo<T> FromType<TService>(DependencyLifecycle lifecycle, string name = null)
            where TService : T
        {
            return new BindingInfo<T>(typeof(TService), null, default(T), lifecycle, name);
        }

        /// <summary>
        ///     Creates an instance of BindingInfo from a type.
        /// </summary>
        public static BindingInfo<T> FromType(Type serviceType, DependencyLifecycle lifecycle, string name = null)
        {
            Should.NotBeNull(serviceType, "serviceType");
            Should.BeOfType<T>(serviceType, "serviceType");
            return new BindingInfo<T>(serviceType, null, default(T), lifecycle, name);
        }

        /// <summary>
        ///     Creates an instance of BindingInfo from a method.
        /// </summary>
        public static BindingInfo<T> FromMethod(Func<IIocContainer, IList<IIocParameter>, T> methodBindingDelegate,
            DependencyLifecycle lifecycle, string name = null)
        {
            Should.NotBeNull(methodBindingDelegate, "methodBindingDelegate");
            return new BindingInfo<T>(null, methodBindingDelegate, default(T), lifecycle, name);
        }

        /// <summary>
        ///     Sets the current binding.
        /// </summary>
        /// <param name="iocContainer"></param>
        public void SetBinding(IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            if (IsEmpty)
                return;
            if (MethodBindingDelegate != null)
                iocContainer.BindToMethod(MethodBindingDelegate, Lifecycle, Name);
            else if (Type != null)
                iocContainer.Bind(typeof(T), Type, Lifecycle, Name);
            else
                iocContainer.BindToConstant(typeof(T), Instance, Name);
        }

        #endregion
    }
}