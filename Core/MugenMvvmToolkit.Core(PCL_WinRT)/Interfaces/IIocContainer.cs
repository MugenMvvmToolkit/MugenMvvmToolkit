#region Copyright

// ****************************************************************************
// <copyright file="IIocContainer.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represent the base interface for ioc container.
    /// </summary>
    public interface IIocContainer : IDisposableObject, IServiceProvider
    {
        /// <summary>
        ///     Gets the id of <see cref="IIocContainer" />.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets the parent ioc adapter.
        /// </summary>
        [CanBeNull]
        IIocContainer Parent { get; }

        /// <summary>
        ///     Gets the original ioc container.
        /// </summary>
        [NotNull]
        object Container { get; }

        /// <summary>
        ///     Creates a child ioc adapter.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IIocContainer" />.
        /// </returns>
        [NotNull]
        IIocContainer CreateChild();

        /// <summary>
        ///     Gets an instance of the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        [Pure]
        object Get([NotNull] Type service, string name = null, params IIocParameter[] parameters);

        /// <summary>
        ///     Gets all instances of the specified service.
        /// </summary>
        /// <param name="service">Specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        /// <returns>An instance of the service.</returns>
        [Pure]
        IEnumerable<object> GetAll([NotNull] Type service, string name = null, params IIocParameter[] parameters);

        /// <summary>
        ///     Indicates that the service should be bound to the specified constant value.
        ///     <param name="service">The specified service type.</param>
        ///     <param name="instance">The specified value.</param>
        ///     <param name="name">The specified binding name.</param>
        /// </summary>
        void BindToConstant([NotNull] Type service, object instance, string name = null);

        /// <summary>
        ///     Indicates that the service should be bound to the specified method.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="methodBindingDelegate">The specified factory delegate.</param>
        /// <param name="lifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="parameters">The specified parameters.</param>
        void BindToMethod([NotNull] Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters);

        /// <summary>
        ///     Indicates that the service should be bound to the specified type.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="typeTo">The specified to type</param>
        /// <param name="name">The specified binding name.</param>
        /// <param name="lifecycle">
        ///     The specified <see cref="DependencyLifecycle" />
        /// </param>
        /// <param name="parameters">The specified parameters.</param>
        void Bind([NotNull] Type service, [NotNull] Type typeTo, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters);

        /// <summary>
        ///     Unregisters all bindings with specified conditions for the specified service.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        void Unbind([NotNull] Type service);

        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="service">The specified service type.</param>
        /// <param name="name">The specified binding name.</param>
        /// <returns>
        ///     <c>True</c> if the specified service has been resolved; otherwise, <c>false</c>.
        /// </returns>
        [Pure]
        bool CanResolve([NotNull] Type service, string name = null);
    }
}