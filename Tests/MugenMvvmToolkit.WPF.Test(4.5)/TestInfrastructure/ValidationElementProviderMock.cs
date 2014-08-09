using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ValidationElementProviderMock : IValidationElementProvider
    {
        #region Properties

        public Func<object, IDictionary<string, IList<IValidationElement>>> GetValidationElements { get; set; }

        #endregion

        #region Implementation of IValidationElementProvider

        /// <summary>
        ///     Gets the series of instances of <see cref="IValidationElement" /> for the specified instance.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <returns>A series of instances of <see cref="IValidationElement" />.</returns>
        IDictionary<string, IList<IValidationElement>> IValidationElementProvider.GetValidationElements(object instance)
        {
            return GetValidationElements(instance);
        }

        #endregion
    }
}