using System.Collections;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the interface that allows to generate items from collection.
    /// </summary>
    public interface IItemsSourceGenerator
    {
        /// <summary>
        ///     Gets the current items source, if any.
        /// </summary>
        [CanBeNull]
        IEnumerable ItemsSource { get; }

                /// <summary>
        ///     Sets the current items source.
        /// </summary>
        void SetItemsSource([CanBeNull] IEnumerable itemsSource, IDataContext context = null);

                /// <summary>
        ///     Resets the current items source.
        /// </summary>
        void Reset();
    }
}