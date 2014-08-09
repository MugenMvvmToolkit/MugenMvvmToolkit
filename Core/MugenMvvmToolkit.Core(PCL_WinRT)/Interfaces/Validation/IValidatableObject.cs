#region Copyright
// ****************************************************************************
// <copyright file="IValidatableObject.cs">
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
using System.Collections.Generic;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Provides a way for an object to be invalidated.
    /// </summary>
    public interface IValidatableObject
    {
        /// <summary>
        ///     Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        ///     A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The validation context.</param>
        IEnumerable<IValidationResult> Validate(IValidationContext validationContext);
    }
}