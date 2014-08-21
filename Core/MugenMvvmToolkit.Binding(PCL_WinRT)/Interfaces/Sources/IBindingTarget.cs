#region Copyright
// ****************************************************************************
// <copyright file="IBindingTarget.cs">
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
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Sources
{
    /// <summary>
    ///     Represents the binding target object.
    /// </summary>
    public interface IBindingTarget : IBindingSource
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this element is enabled in the user interface (UI).
        /// </summary>
        /// <returns>
        ///     true if the element is enabled; otherwise, false.
        /// </returns>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets a value that indicates whether the target supports the validation.
        /// </summary>
        /// <returns>
        ///     true if the target is validatable; otherwise false.
        /// </returns>
        bool Validatable { get; }

        /// <summary>
        ///     Gets a parameter to pass to the command.
        /// </summary>
        /// <returns>
        ///     Parameter to pass to the command.
        /// </returns>
        [CanBeNull]
        object GetCommandParameter(IDataContext context);

        /// <summary>
        ///     Sets errors for target.
        /// </summary>
        /// <param name="senderType">The source of the errors.</param>
        /// <param name="errors">The collection of errors</param>
        void SetErrors(SenderType senderType, [CanBeNull] IList<object> errors);
    }
}