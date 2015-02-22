using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Infrastructure.Validation;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    internal class ValidationElementMock : DataAnnotationValidatior.IValidationElement
    {
        #region Properties

        internal Func<DataAnnotationValidatior.ValidationContext, IEnumerable<object>> Validate { get; set; }

        #endregion

        #region Implementation of IValidationElement

        /// <summary>
        ///     Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        ///     A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The context information about the validation operation.</param>
        IEnumerable<object> DataAnnotationValidatior.IValidationElement.Validate(
            DataAnnotationValidatior.ValidationContext validationContext)
        {
            if (Validate == null)
                return Enumerable.Empty<object>();
            return Validate(validationContext);
        }

        #endregion
    }
}