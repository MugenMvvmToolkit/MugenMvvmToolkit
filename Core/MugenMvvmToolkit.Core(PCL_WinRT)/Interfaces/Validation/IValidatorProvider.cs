#region Copyright
// ****************************************************************************
// <copyright file="IValidatorProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents the factory that allows to create an instance of <see cref="IValidator" />.
    /// </summary>
    public interface IValidatorProvider : IDisposableObject
    {
        /// <summary>
        ///     Registers the specified validator using the type.
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        void Register<TValidator>() where TValidator : IValidator;

        /// <summary>
        ///     Registers the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator</param>
        void Register([NotNull] IValidator validator);

        /// <summary>
        ///     Unregisters the specified validator use type.
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        bool Unregister<TValidator>() where TValidator : IValidator;

        /// <summary>
        ///     Determines whether the specified validator is registered
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        [Pure]
        bool IsRegistered<TValidator>() where TValidator : IValidator;

        /// <summary>
        ///     Determines whether the specified validator is registered
        /// </summary>
        [Pure]
        bool IsRegistered(Type type);

        /// <summary>
        ///     Gets the series of validators for the specified instance.
        /// </summary>
        /// <param name="context">The specified IValidatorContext.</param>
        /// <returns>A series instances of validators.</returns>
        [NotNull]
        IList<IValidator> GetValidators([NotNull] IValidatorContext context);

        /// <summary>
        ///     Creates an instance of <see cref="IValidatorAggregator" />.
        /// </summary>
        [NotNull]
        IValidatorAggregator GetValidatorAggregator();        
    }
}