using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        public int ValidateCount { get; set; }

        public IList<string> ValidateProperties { get; set; }

        public int ValidateAllCount { get; set; }

        public int IsValidCount { get; set; }

        public int ClearPropertyErrorsCount { get; set; }

        public int ClearAllErrorsCount { get; set; }

        #endregion

        #region Overrides of ValidatorBase

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
        /// <param name="propertyName">The specified property name.</param>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName)
        {
            ValidateProperties.Add(propertyName);
            ValidateCount++;
            return base.ValidateInternalAsync(propertyName);
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync()
        {
            ValidateAllCount++;
            return base.ValidateInternalAsync();
        }
        /// <summary>
        ///     Creates a new validator that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator that is a copy of this instance.
        /// </returns>
        protected override IValidator CloneInternal()
        {
            return new SpyValidator();
        }

        #endregion
    }
}