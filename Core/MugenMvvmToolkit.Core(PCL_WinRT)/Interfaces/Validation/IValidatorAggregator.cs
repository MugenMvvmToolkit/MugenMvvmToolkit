#region Copyright
// ****************************************************************************
// <copyright file="IValidatorAggregator.cs">
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
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents the interface that allows to aggregate validators and use it as one.
    /// </summary>
    public interface IValidatorAggregator : IDisposableObject, INotifyDataErrorInfo
#if NONOTIFYDATAERROR
      ,IDataErrorInfo  
#endif
    {
        /// <summary>
        ///     Gets or sets the delegate that allows to create an instance of <see cref="IValidatorContext" />.
        /// </summary>
        [NotNull]
        Func<object, IValidatorContext> CreateContext { get; set; }

        /// <summary>
        ///     Gets the mapping of model properties.
        ///     <example>
        ///         <code>
        ///       <![CDATA[
        ///        PropertyMappings.Add("ModelProperty", new[]{"ViewModelProperty"});
        ///       ]]>
        ///     </code>
        ///     </example>
        /// </summary>
        [NotNull]
        IDictionary<string, ICollection<string>> PropertyMappings { get; }

        /// <summary>
        ///     Gets the list of properties that will not be validated.
        /// </summary>
        [NotNull]
        ICollection<string> IgnoreProperties { get; }

        /// <summary>
        ///     Gets the validator that allows to set errors manually.
        /// </summary>
        ManualValidator Validator { get; }

        /// <summary>
        ///     Determines whether the current validator is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current validator is valid, otherwise <c>false</c>.
        /// </returns>
        bool IsValid { get; }

        /// <summary>
        ///     Gets the collection of validators.
        /// </summary>
        [NotNull]
        IList<IValidator> GetValidators();

        /// <summary>
        ///     Adds the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
        void AddValidator([NotNull] IValidator validator);

        /// <summary>
        ///     Removes the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
        bool RemoveValidator([NotNull] IValidator validator);

        /// <summary>
        ///     Adds the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        void AddInstance([NotNull] object instanceToValidate);

        /// <summary>
        ///     Removes the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        bool RemoveInstance([NotNull] object instanceToValidate);

        /// <summary>
        ///     Updates information about errors in the specified instance.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        [SuppressTaskBusyHandler, NotNull]
        Task ValidateInstanceAsync([NotNull] object instanceToValidate);

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        [SuppressTaskBusyHandler, NotNull]
        Task ValidateAsync([NotNull] string propertyName);

        /// <summary>
        ///     Updates information about errors.
        /// </summary>
        [SuppressTaskBusyHandler, NotNull]
        Task ValidateAsync();

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
        new IList<object> GetErrors(string propertyName);

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
        [NotNull]
        IDictionary<string, IList<object>> GetErrors();

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        void ClearErrors([NotNull] string propertyName);

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        void ClearErrors();
    }
}