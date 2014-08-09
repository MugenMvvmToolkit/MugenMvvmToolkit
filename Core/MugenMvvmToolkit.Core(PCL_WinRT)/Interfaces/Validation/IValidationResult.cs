#region Copyright
// ****************************************************************************
// <copyright file="IValidationResult.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    /// <summary>
    ///     Represents a container for the results of a validation request.
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        ///     Gets the value which contains validation result.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        ///     Gets the collection of member names that indicate which fields have validation errors.
        /// </summary>
        /// <returns>
        ///     The collection of member names that indicate which fields have validation errors.
        /// </returns>
        [NotNull]
        IEnumerable<string> MemberNames { get; }

        /// <summary>
        ///     Gets the error message for the validation.
        /// </summary>
        /// <returns>
        ///     The error message for the validation.
        /// </returns>
        string ErrorMessage { get; }
    }
}