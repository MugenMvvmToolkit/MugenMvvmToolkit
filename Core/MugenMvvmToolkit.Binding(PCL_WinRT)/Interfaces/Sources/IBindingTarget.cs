#region Copyright

// ****************************************************************************
// <copyright file="IBindingTarget.cs">
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

using JetBrains.Annotations;
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
        ///     Gets a parameter to pass to the command.
        /// </summary>
        /// <returns>
        ///     Parameter to pass to the command.
        /// </returns>
        [CanBeNull]
        object GetCommandParameter(IDataContext context);
    }
}