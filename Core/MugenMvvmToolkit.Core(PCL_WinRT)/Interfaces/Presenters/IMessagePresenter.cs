#region Copyright

// ****************************************************************************
// <copyright file="IMessagePresenter.cs">
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

using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    /// <summary>
    ///     Represent the base interface for message box implementation.
    /// </summary>
    public interface IMessagePresenter
    {
        /// <summary>
        ///     Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message
        ///     box result and returns a result.
        /// </summary>
        /// <param name="message">A <see cref="T:System.String" /> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="T:System.String" /> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageButton" /> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageImage" /> value that specifies the icon to display.</param>
        /// <param name="defaultResult">
        ///     A <see cref="MessageResult" /> value that specifies the default result of the message
        ///     box.
        /// </param>
        /// <param name="context">The specified context.</param>
        /// <returns>A <see cref="MessageResult" /> value that specifies which message box button is clicked by the user.</returns>
        [SuppressTaskBusyHandler, NotNull]
        Task<MessageResult> ShowAsync(string message, string caption = "",
            MessageButton button = MessageButton.Ok,
            MessageImage icon = MessageImage.None, MessageResult defaultResult = MessageResult.None, IDataContext context = null);
    }
}