#region Copyright
// ****************************************************************************
// <copyright file="BindingTarget.cs">
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
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Sources
{
    /// <summary>
    ///     Represents the validatable, focusable and command support binding source.
    /// </summary>
    public class BindingTarget : BindingSource, IBindingTarget
    {
        #region Nested types

        private sealed class ErrorsDictionary : LightDictionaryBase<SenderType, IList<object>>
        {
            #region Constructors

            public ErrorsDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<SenderType,IList<object>>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(SenderType x, SenderType y)
            {
                return x.Equals(y);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(SenderType key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        private ErrorsDictionary _errors;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingTarget" /> class.
        /// </summary>
        public BindingTarget([NotNull] IObserver observer)
            : base(observer)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the delegate that uses as parameter to pass to the command.
        /// </summary>
        public Func<IDataContext, object> CommandParameterDelegate { get; set; }

        #endregion

        #region Implementation of IBindingTarget

        /// <summary>
        ///     Gets or sets a value indicating whether this element is enabled in the user interface (UI).
        /// </summary>
        /// <returns>
        ///     true if the element is enabled; otherwise, false.
        /// </returns>
        public bool IsEnabled
        {
            get
            {
                object penultimateValue = Observer.GetPathMembers(false).PenultimateValue;
                if (penultimateValue == null || penultimateValue.IsUnsetValue())
                    return false;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(penultimateValue.GetType(), AttachedMemberConstants.Enabled, false, false);
                if (member == null)
                    return false;
                return (bool)member.GetValue(penultimateValue, null);
            }
            set
            {
                object penultimateValue = Observer.GetPathMembers(false).PenultimateValue;
                if (penultimateValue == null || penultimateValue.IsUnsetValue())
                    return;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(penultimateValue.GetType(), AttachedMemberConstants.Enabled, false, false);
                if (member == null)
                    Tracer.Warn("The member {0} cannot be obtained on type {1}",
                        AttachedMemberConstants.Enabled, penultimateValue.GetType());
                else
                    member.SetValue(penultimateValue, new object[] { value });
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the target supports the validation.
        /// </summary>
        /// <returns>
        ///     true if the target is validatable; otherwise false.
        /// </returns>
        public bool Validatable
        {
            get
            {
                var target = Observer.GetPathMembers(false).PenultimateValue;
                return target != null && !target.IsUnsetValue();
            }
        }

        /// <summary>
        ///     Gets a parameter to pass to the command.
        /// </summary>
        /// <returns>
        ///     Parameter to pass to the command.
        /// </returns>
        public object GetCommandParameter(IDataContext context)
        {
            var @delegate = CommandParameterDelegate;
            if (@delegate != null)
                return @delegate(context);
            object target = Observer.GetPathMembers(false).PenultimateValue;
            if (target == null || target.IsUnsetValue())
                return null;

            var commandParameterMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), AttachedMemberConstants.CommandParameter, false, false);
            if (commandParameterMember == null)
                return null;
            return commandParameterMember.GetValue(target, new object[] { context });
        }

        /// <summary>
        ///     Sets errors for target.
        /// </summary>
        /// <param name="senderType">The source of the errors.</param>
        /// <param name="errors">The collection of errors</param>
        public void SetErrors(SenderType senderType, IList<object> errors)
        {
            var source = Observer.GetPathMembers(false).PenultimateValue;
            if (source == null || source.IsUnsetValue())
                return;

            var errorProvider = BindingServiceProvider.ErrorProvider;
            IBindingMemberInfo propertyMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), AttachedMemberConstants.ErrorsPropertyMember, false, false);
            if (errorProvider == null && propertyMember == null)
                return;
            lock (Observer)
            {
                if (_errors == null)
                    _errors = new ErrorsDictionary();
                if (errors == null || errors.Count == 0)
                    _errors.Remove(senderType);
                else
                    _errors[senderType] = errors;
                if (_errors.Count == 0)
                    errors = Empty.Array<object>();
                else if (_errors.Count == 1)
                    errors = _errors.FirstOrDefault().Value;
                else
                    errors = _errors.SelectMany(list => list.Value).ToList();
            }

            if (errorProvider != null)
                errorProvider.SetErrors(source, errors);

            if (propertyMember != null && propertyMember.CanWrite)
                propertyMember.SetValue(source, new object[] { errors });
        }

        #endregion
    }
}