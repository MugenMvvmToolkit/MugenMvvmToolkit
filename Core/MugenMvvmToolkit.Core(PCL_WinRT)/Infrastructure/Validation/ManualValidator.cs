#region Copyright

// ****************************************************************************
// <copyright file="ManualValidator.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents a simple validator that does not have validation logic, the user sets all the errors.
    /// </summary>
    public class ManualValidator<T> : ValidatorBase<T>
    {
        #region Methods

        /// <summary>
        ///     Sets errors for a property
        /// </summary>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors<TValue>(Expression<Func<TValue>> propertyExpresssion, params object[] errors)
        {
            UpdateErrors(propertyExpresssion.GetMemberInfo().Name, errors, false);
        }

        /// <summary>
        ///     Sets errors for a property
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors<TModel>(Expression<Func<TModel, object>> propertyExpresssion, params object[] errors)
        {
            UpdateErrors(ToolkitExtensions.GetMemberName(propertyExpresssion), errors, false);
        }

        /// <summary>
        ///     Sets errors for a property
        /// </summary>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors<TValue>(Expression<Func<T, TValue>> propertyExpresssion, params object[] errors)
        {
            UpdateErrors(ToolkitExtensions.GetMemberName(propertyExpresssion), errors, false);
        }

        /// <summary>
        ///     Set errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(string propertyName, params object[] errors)
        {
            UpdateErrors(propertyName, errors, false);
        }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <returns> The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            return DoNothingResult;
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>The result of validation.</returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            return DoNothingResult;
        }

        #endregion
    }

    /// <summary>
    ///     Represents a simple validator that does not have validation logic, the user sets all the errors.
    /// </summary>
    public class ManualValidator : ManualValidator<object>
    {
    }
}