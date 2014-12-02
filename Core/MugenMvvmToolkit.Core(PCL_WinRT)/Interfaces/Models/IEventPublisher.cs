using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the object that allows to send messages.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        void Publish([NotNull] object sender, [NotNull] object message);
    }
}