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