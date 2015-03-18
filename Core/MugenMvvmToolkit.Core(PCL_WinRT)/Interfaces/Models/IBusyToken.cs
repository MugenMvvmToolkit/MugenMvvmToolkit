using System;

namespace MugenMvvmToolkit.Interfaces.Models
{
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
        void Register(IHandler<IBusyToken> handler);
    }
}