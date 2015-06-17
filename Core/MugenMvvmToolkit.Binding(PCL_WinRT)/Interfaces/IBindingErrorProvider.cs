#region Copyright

// ****************************************************************************
// <copyright file="IBindingErrorProvider.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the interfaces that provides a user interface for indicating that a control on a form has an error associated with it.
    /// </summary>
    public interface IBindingErrorProvider
    {
        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="key">
        ///     The name of the key to retrieve validation errors for; or null or
        ///     <see cref="F:System.String.Empty" />, to retrieve entity-level errors.
        /// </param>
        /// <param name="context">The specified context, if any.</param>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        [NotNull]
        IList<object> GetErrors([NotNull]object target, string key, [CanBeNull] IDataContext context);

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="senderKey">The source of the errors.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        void SetErrors([NotNull]object target, [NotNull] string senderKey, [NotNull] IList<object> errors,
            [CanBeNull] IDataContext context);
    }
}