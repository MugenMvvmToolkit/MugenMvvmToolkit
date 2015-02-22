#region Copyright

// ****************************************************************************
// <copyright file="IValidatorProvider.cs">
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

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents the factory that allows to create an instance of <see cref="IValidator" />.
    /// </summary>
    public interface IValidatorProvider
    {
        /// <summary>
        ///     Registers the specified validator.
        /// </summary>
        void Register([NotNull] Type validatorType);

        /// <summary>
        ///     Determines whether the specified validator is registered
        /// </summary>
        [Pure]
        bool IsRegistered([NotNull] Type validatorType);

        /// <summary>
        ///     Unregisters the specified validator.
        /// </summary>
        bool Unregister([NotNull] Type validatorType);

        /// <summary>
        ///     Gets the series of validator types.
        /// </summary>
        [NotNull]
        IList<Type> GetValidatorTypes();

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