#region Copyright

// ****************************************************************************
// <copyright file="IEventListener.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the event listener interface.
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        ///     Gets the value that indicates that the listener is weak.
        ///     <c>true</c> the listener can be used without <c>WeakReference</c>.
        ///     <c>false</c> the listener should be wrapped to <c>WeakReference</c>.
        /// </summary>
        bool IsWeak { get; }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void Handle(object sender, object message);
    }
}