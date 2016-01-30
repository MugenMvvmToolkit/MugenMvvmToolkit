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
