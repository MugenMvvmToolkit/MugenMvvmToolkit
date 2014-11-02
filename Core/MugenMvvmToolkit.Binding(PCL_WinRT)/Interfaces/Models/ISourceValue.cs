using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface of source value wrapper.
    /// </summary>
    public interface ISourceValue
    {
        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ISourceValue" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ISourceValue" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        bool IsAlive { get; }

        /// <summary>
        ///     Gets the current source value.
        /// </summary>
        [CanBeNull]
        object Value { get; }

        /// <summary>
        ///     Occurs when the <see cref="Value"/>  property changed.
        /// </summary>
        event EventHandler<ISourceValue, EventArgs> ValueChanged;
    }
}