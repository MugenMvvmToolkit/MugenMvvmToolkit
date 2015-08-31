#region Copyright

// ****************************************************************************
// <copyright file="IValidator.cs">
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
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents a validator.
    /// </summary>
    public interface IValidator : IDisposable, INotifyDataErrorInfo
#if NONOTIFYDATAERROR
, IDataErrorInfo
#endif
    {
        /// <summary>
        ///     Gets or sets the value, that indicates that the validator will be validate property on changed. Default is true.
        /// </summary>
        bool ValidateOnPropertyChanged { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this validator is disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
        bool IsValid { get; }

        /// <summary>
        ///     Gets the validator context.
        /// </summary>
        [CanBeNull]
        IValidatorContext Context { get; }

        /// <summary>
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        bool Initialize([NotNull] IValidatorContext context);

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or
        ///     <see cref="F:System.String.Empty" />, to retrieve entity-level errors.
        /// </param>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        [NotNull]
        new IList<object> GetErrors([CanBeNull]string propertyName);

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
        [NotNull, Pure]
        IDictionary<string, IList<object>> GetErrors();

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        [NotNull]
        Task ValidateAsync([CanBeNull] string propertyName);

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        [NotNull]
        Task ValidateAsync();

        /// <summary>
        ///     Tries to cancel the current validation process.
        /// </summary>
        void CancelValidation();

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        void ClearErrors([CanBeNull]string propertyName);

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        void ClearErrors();
    }
}