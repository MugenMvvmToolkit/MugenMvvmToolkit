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

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents a validator.
    /// </summary>
    public interface IValidator : IDisposableObject, IObservable, INotifyDataErrorInfo
#if NONOTIFYDATAERROR
      ,IDataErrorInfo  
#endif
    {
        /// <summary>
        ///     Gets a value indicating whether an attempt to add a duplicate validator to the collection will cause an exception to be thrown.
        /// </summary>
        bool AllowDuplicate { get; }

        /// <summary>
        ///     Gets the initialized state of the validator.
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
        ///     Gets or sets the value, that indicates that the validator will be validate property on changed. Default is true.
        /// </summary>
        bool ValidateOnPropertyChanged { get; set; }

        /// <summary>
        ///     Gets the validator context.
        /// </summary>
        IValidatorContext Context { get; }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        bool CanValidate([NotNull] IValidatorContext validatorContext);

        /// <summary>
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        void Initialize([NotNull] IValidatorContext context);

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
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        void ClearErrors([CanBeNull]string propertyName);

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        void ClearErrors();

        /// <summary>
        ///     Creates a new validator that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator that is a copy of this instance.
        /// </returns>
        [NotNull]
        IValidator Clone();
    }
}