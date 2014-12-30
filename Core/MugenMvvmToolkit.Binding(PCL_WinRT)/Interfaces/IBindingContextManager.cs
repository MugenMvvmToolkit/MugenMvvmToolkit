#region Copyright

// ****************************************************************************
// <copyright file="IBindingContextManager.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the binding context manager.
    /// </summary>
    public interface IBindingContextManager
    {
        /// <summary>
        ///     Gets a value indicating whether the item has binding context.
        /// </summary>
        bool HasBindingContext([NotNull] object item);

        /// <summary>
        ///     Gets the binding context for the specified item.
        /// </summary>
        [NotNull]
        IBindingContext GetBindingContext([NotNull] object item);
    }
}