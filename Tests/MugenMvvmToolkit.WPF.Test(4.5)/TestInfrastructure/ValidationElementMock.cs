using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ValidationElementMock : IValidationElement
    {
        #region Properties

        public Func<IValidationContext, IEnumerable<IValidationResult>> Validate { get; set; }

        #endregion

        #region Implementation of IValidationElement

        /// <summary>
        ///     Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        ///     A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The context information about the validation operation.</param>
        IEnumerable<IValidationResult> IValidationElement.Validate(IValidationContext validationContext)
        {
            if (Validate == null)
                return Enumerable.Empty<IValidationResult>();
            return Validate(validationContext);
        }

        #endregion
    }
}