#region Copyright

// ****************************************************************************
// <copyright file="ValidationResult.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.Validation
{
    /// <summary>
    ///     Represents a container for the results of a validation request.
    /// </summary>
    [Serializable]
    public class ValidationResult : IValidationResult
    {
        #region Fields

        /// <summary>
        ///     Represents the success of the validation (true if validation was successful; otherwise, false).
        /// </summary>
        public static readonly ValidationResult Success;

        private readonly string _errorMessage;
        private readonly IEnumerable<string> _memberNames;

        #endregion

        #region Constructors

        static ValidationResult()
        {
            Success = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResult" /> class by using an error message and a list of
        ///     members that have validation errors.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="memberNames">The list of member names that have validation errors.</param>
        public ValidationResult(string errorMessage, IEnumerable<string> memberNames = null)
        {
            _errorMessage = errorMessage;
            _memberNames = memberNames ?? Enumerable.Empty<string>();
        }

        #endregion

        #region Implementation of IValidationResult

        /// <summary>
        ///     Gets the value which contains validation result.
        /// </summary>
        public bool IsValid
        {
            get { return string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        /// <summary>
        ///     Gets the collection of member names that indicate which fields have validation errors.
        /// </summary>
        /// <returns>
        ///     The collection of member names that indicate which fields have validation errors.
        /// </returns>
        public IEnumerable<string> MemberNames
        {
            get { return _memberNames; }
        }

        /// <summary>
        ///     Gets the error message for the validation.
        /// </summary>
        /// <returns>
        ///     The error message for the validation.
        /// </returns>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string representation of the current validation result.
        /// </summary>
        /// <returns>
        ///     The current validation result.
        /// </returns>
        public override string ToString()
        {
            return ErrorMessage ?? base.ToString();
        }

        #endregion
    }
}