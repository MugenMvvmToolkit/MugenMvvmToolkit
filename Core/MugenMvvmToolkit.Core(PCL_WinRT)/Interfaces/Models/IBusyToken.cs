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