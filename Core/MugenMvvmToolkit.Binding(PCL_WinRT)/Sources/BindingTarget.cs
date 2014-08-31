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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Sources
{
    /// <summary>
    ///     Represents the validatable, focusable and command support binding source.
    /// </summary>
    public class BindingTarget : BindingSource, IBindingTarget
    {
        
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

        #endregion
    }
}