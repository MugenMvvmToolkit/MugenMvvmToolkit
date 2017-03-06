#region Copyright

// ****************************************************************************
// <copyright file="ValidationElementMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
