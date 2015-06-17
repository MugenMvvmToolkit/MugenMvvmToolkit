#region Copyright

// ****************************************************************************
// <copyright file="IBusyToken.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents a busy token registration.
    /// </summary>
    public interface IBusyToken : IDisposable
    {
        /// <summary>
        ///     Gets whether this <see cref="IBusyToken" /> has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        object Message { get; }

        /// <summary>
        ///     Registers a delegate that will be called when this <see cref="IBusyToken" /> is completed.
        /// </summary>
        void Register(IBusyTokenCallback callback);
    }

    /// <summary>
    ///     Represents a busy token callback.
    /// </summary>
    public interface IBusyTokenCallback
    {
        /// <summary>
        ///     This method will be invoked when the busy token is completed.
        /// </summary>
        void OnCompleted(IBusyToken token);
    }
}