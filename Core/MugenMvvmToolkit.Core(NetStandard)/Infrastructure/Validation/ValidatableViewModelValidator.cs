#region Copyright

// ****************************************************************************
// <copyright file="ValidatableViewModelValidator.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public class ValidatableViewModelValidator : ValidatorBase<IValidatableViewModel>, IValidatableViewModelValidator
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

        protected override IList<object> GetErrorsInternal(string propertyName)
        {
            return Instance.GetErrors(propertyName);
        }

        protected override bool IsValidInternal()
        {
            return Instance.IsValid;
        }

        protected override void ClearErrorsInternal(string propertyName)
        {
            Instance.ClearErrors(propertyName);
        }

        protected override void ClearErrorsInternal()
        {
            Instance.ClearErrors();
        }

        protected override void OnInitialized(IValidatorContext context)
        {
            _weakHandler = ReflectionExtensions.MakeWeakErrorsChangedHandler(this, ViewModelErrorsChanged);
            Instance.ErrorsChanged += _weakHandler;
        }

        protected override bool CanValidateInternal(IValidatorContext validatorContext)
        {
            IViewModel viewModel;
            validatorContext.ValidationMetadata.TryGetData(ValidationConstants.ViewModel, out viewModel);
            return viewModel == null || !ReferenceEquals(viewModel, validatorContext.Instance);
        }

        protected override void OnDispose()
        {
            if (_weakHandler != null && Instance != null)
            {
                Instance.ErrorsChanged -= _weakHandler;
                _weakHandler = null;
            }
        }

        protected override IDictionary<string, IList<object>> GetErrorsInternal()
        {
            return Instance.GetErrors();
        }

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            Instance.ValidateAsync(propertyName);
            return DoNothingResult;
        }

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            Instance.ValidateAsync();
            return DoNothingResult;
        }

        #endregion
    }
}
