#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProviderBase.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class BindingErrorProviderBase : IBindingErrorProvider
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
                return x.Equals(y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly BindingMemberDescriptor<object, object> ErrorsDescriptor;
        private const string ErrorsKey = "@#@er";

        #endregion

        #region Constructors

        static BindingErrorProviderBase()
        {
            ErrorsDescriptor = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.ErrorsPropertyMember);
        }

        #endregion

        #region Implementation of IBindingErrorProvider

        public IList<object> GetErrors(object target, string key, IDataContext context)
        {
            Should.NotBeNull(target, nameof(target));
            var dictionary = GetErrorsDictionary(target);
            if (dictionary == null)
                return Empty.Array<object>();
            if (string.IsNullOrEmpty(key))
                return dictionary.SelectMany(pair => pair.Value).ToList();
            IList<object> list;
            if (dictionary.TryGetValue(key, out list))
                return list;
            return Empty.Array<object>();
        }

        public void SetErrors(object target, string senderKey, IList<object> errors, IDataContext context)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(senderKey, nameof(senderKey));
            if (context == null)
                context = DataContext.Empty;
            var dict = GetOrAddErrorsDictionary(target);
            if (context.GetData(BindingConstants.ClearErrors) && (dict.Count == 0 || (dict.Count == 1 && dict.ContainsKey(senderKey))))
            {
                ServiceProvider.AttachedValueProvider.Clear(target, ErrorsKey);
                ClearErrors(target, context);
                target.TryRaiseAttachedEvent(ErrorsDescriptor);
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
            target.TryRaiseAttachedEvent(ErrorsDescriptor);
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

        protected abstract void SetErrors([NotNull] object target, [NotNull] IList<object> errors, [NotNull] IDataContext context);

        protected virtual void ClearErrors([NotNull] object target, [NotNull] IDataContext context)
        {
        }

        #endregion
    }
}
