#region Copyright
// ****************************************************************************
// <copyright file="ManualValidator.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents the simple validator that not has validation logic, user sets all erros.
    /// </summary>
    public class ManualValidator<T> : ValidatorBase<T>
    {
        #region Methods

        /// <summary>
        ///     Sets errors for a property
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors<TModel>(Expression<Func<TModel, object>> propertyExpresssion, params object[] errors)
        {
            UpdateErrors(Extensions.GetPropertyName(propertyExpresssion), errors, false);
        }

        /// <summary>
        ///     Sets errors for a property
        /// </summary>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(Expression<Func<T, object>> propertyExpresssion, params object[] errors)
        {
            UpdateErrors(Extensions.GetPropertyName(propertyExpresssion), errors, false);
        }

        /// <summary>
        ///     Set errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(string propertyName, params object[] errors)
        {
            UpdateErrors(propertyName, errors, false);
        }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Indicates that can be only once instance of this validator.
        /// </summary>
        public override bool IsUnique
        {
            get { return false; }
        }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        protected override bool CanValidateInternal(IValidatorContext validatorContext)
        {
            return true;
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName)
        {
            return DoNothingResult;
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync()
        {
            return DoNothingResult;
        }

        /// <summary>
        ///     Creates a new validator that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator that is a copy of this instance.
        /// </returns>
        protected override IValidator CloneInternal()
        {
            return new ManualValidator<T>();
        }

        #endregion
    }

    /// <summary>
    ///     Represents the simple validator that not has validation logic, user sets all erros.
    /// </summary>
    public class ManualValidator : ManualValidator<object>
    {
    }
}