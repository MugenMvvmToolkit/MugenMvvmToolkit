#region Copyright

// ****************************************************************************
// <copyright file="ManualValidator.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public class ManualValidator<T> : ValidatorBase<T>
    {
        #region Methods

        public void SetErrors<TModel>(Func<Expression<Func<TModel, object>>> memberExpresssion, params object[] errors)
        {
            UpdateErrors(memberExpresssion.GetMemberName(), errors, false);
        }

        public void SetErrors(Func<Expression<Func<T, object>>> memberExpresssion, params object[] errors)
        {
            UpdateErrors(memberExpresssion.GetMemberName(), errors, false);
        }

        public void SetErrors(string propertyName, params object[] errors)
        {
            UpdateErrors(propertyName, errors, false);
        }

        #endregion

        #region Overrides of ValidatorBase

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token)
        {
            return DoNothingResult;
        }

        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token)
        {
            return DoNothingResult;
        }

        #endregion
    }

    public class ManualValidator : ManualValidator<object>
    {
    }
}
