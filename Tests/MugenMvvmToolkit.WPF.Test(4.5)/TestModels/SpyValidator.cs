using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class SpyValidator : ManualValidator<object>
    {
        #region Constructors

        public SpyValidator()
        {
            ValidateProperties = new List<string>();
        }

        #endregion

        #region Properties

        public Func<IValidatorContext, bool> CanValidate { get; set; }

        public int ValidateCount { get; set; }

        public IList<string> ValidateProperties { get; set; }

        public int ValidateAllCount { get; set; }

        public int IsValidCount { get; set; }

        public int ClearPropertyErrorsCount { get; set; }

        public int ClearAllErrorsCount { get; set; }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        protected override bool CanValidateInternal(IValidatorContext validatorContext)
        {
            if (CanValidate == null)
                return base.CanValidateInternal(validatorContext);
            return CanValidate(validatorContext);
        }

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
        protected override bool IsValidInternal()
        {
            IsValidCount++;
            return base.IsValidInternal();
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected override void ClearErrorsInternal(string propertyName)
        {
            base.ClearErrorsInternal(propertyName);
            ClearPropertyErrorsCount++;
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        protected override void ClearErrorsInternal()
        {
            base.ClearErrorsInternal();
            ClearAllErrorsCount++;
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <returns> The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            ValidateProperties.Add(propertyName);
            ValidateCount++;
            return base.ValidateInternalAsync(propertyName, token);
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            ValidateAllCount++;
            return base.ValidateInternalAsync(token);
        }

        #endregion
    }
}