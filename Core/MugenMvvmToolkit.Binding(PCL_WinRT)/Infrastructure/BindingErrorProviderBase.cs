#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProviderBase.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated with it.
    /// </summary>
    public class BindingErrorProviderBase : IBindingErrorProvider
    {
        #region Nested types

        protected sealed class ErrorsDictionary : LightDictionaryBase<string, IList<object>>
        {
            #region Constructors

            public ErrorsDictionary()
                : base(true)
            {
            }

            #endregion

            #region Methods

            public new bool TryGetValue(string key, out IList<object> result)
            {
                return base.TryGetValue(key, out result);
            }

            public new bool ContainsKey(string key)
            {
                return base.ContainsKey(key);
            }

            #endregion

            #region Overrides of LightDictionaryBase<SenderType,IList<object>>

            protected override bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        public static readonly DataConstant<bool> ClearErrorsConstant;
        private const string ErrorsKey = "@#@er";

        #endregion

        #region Constructors

        static BindingErrorProviderBase()
        {
            ClearErrorsConstant = DataConstant.Create(() => ClearErrorsConstant);
        }

        #endregion

        #region Implementation of IBindingErrorProvider

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="senderKey">The source of the errors.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        public void SetErrors(object target, string senderKey, IList<object> errors, IDataContext context)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(senderKey, "senderKey");
            if (context == null)
                context = DataContext.Empty;
            var dict = GetOrAddErrorsDictionary(target);
            if (context.GetData(ClearErrorsConstant) && (dict.Count == 0 || (dict.Count == 1 && dict.ContainsKey(senderKey))))
            {
                ServiceProvider.AttachedValueProvider.Clear(target, ErrorsKey);
                ClearErrors(target, context);
                return;
            }

            if (errors == null || errors.Count == 0)
                dict.Remove(senderKey);
            else
                dict[senderKey] = errors;
            if (dict.Count == 0)
                errors = Empty.Array<object>();
            else if (dict.Count == 1)
                errors = dict.FirstOrDefault().Value;
            else
                errors = dict.SelectMany(list => list.Value).ToList();
            SetErrors(target, errors, context);
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected static ErrorsDictionary GetErrorsDictionary(object target)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<ErrorsDictionary>(target, ErrorsKey, false);
        }

        private static ErrorsDictionary GetOrAddErrorsDictionary(object target)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(target, ErrorsKey, (o, o1) => new ErrorsDictionary(), null);
        }

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        protected virtual void SetErrors([NotNull] object target, [NotNull] IList<object> errors, [NotNull] IDataContext context)
        {
            var errorsMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false);
            if (errorsMember != null && errorsMember.CanWrite)
                errorsMember.SetValue(target, new object[] { errors });
        }

        /// <summary>
        ///     Clears the errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="context">The specified context, if any.</param>
        protected virtual void ClearErrors([NotNull] object target, [NotNull] IDataContext context)
        {
            SetErrors(target, Empty.Array<object>(), context);
        }

        #endregion
    }
}