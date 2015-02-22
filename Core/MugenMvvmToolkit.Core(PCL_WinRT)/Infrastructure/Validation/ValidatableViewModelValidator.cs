#region Copyright

// ****************************************************************************
// <copyright file="ValidatableViewModelValidator.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents a class that allows to validate the <see cref="IValidatableViewModel"/>.
    /// </summary>
    public class ValidatableViewModelValidator : ValidatorBase<IValidatableViewModel>
    {
        #region Fields

        private EventHandler<DataErrorsChangedEventArgs> _weakHandler;

        #endregion

        #region Methods

        private static void ViewModelErrorsChanged(ValidatableViewModelValidator validator, object o, DataErrorsChangedEventArgs args)
        {
            validator.RaiseErrorsChanged(args.PropertyName, true);
        }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to
        ///     retrieve entity-level errors.
        /// </param>
        protected override IList<object> GetErrorsInternal(string propertyName)
        {
            return Instance.GetErrors(propertyName);
        }

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
        protected override bool IsValidInternal()
        {
            return Instance.IsValid;
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected override void ClearErrorsInternal(string propertyName)
        {
            Instance.ClearErrors(propertyName);
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        protected override void ClearErrorsInternal()
        {
            Instance.ClearErrors();
        }

        /// <summary>
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        protected override void OnInitialized(IValidatorContext context)
        {
            _weakHandler = ReflectionExtensions.MakeWeakErrorsChangedHandler(this, ViewModelErrorsChanged);
            Instance.ErrorsChanged += _weakHandler;
        }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        protected override bool CanValidateInternal(IValidatorContext validatorContext)
        {
            IViewModel viewModel;
            validatorContext.ValidationMetadata.TryGetData(ViewModelConstants.ViewModel, out viewModel);
            return viewModel == null || !ReferenceEquals(viewModel, validatorContext.Instance);
        }

        /// <summary>
        ///     Occurs after current view model disposed, use for clear resource and event listeners.
        /// </summary>
        protected override void OnDispose()
        {
            if (_weakHandler != null && Instance != null)
            {
                Instance.ErrorsChanged -= _weakHandler;
                _weakHandler = null;
            }
        }

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
        protected override IDictionary<string, IList<object>> GetErrorsInternal()
        {
            return Instance.GetErrors();
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <returns> The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            Instance.ValidateAsync(propertyName);
            return DoNothingResult;
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            Instance.ValidateAsync();
            return DoNothingResult;
        }

        #endregion
    }
}