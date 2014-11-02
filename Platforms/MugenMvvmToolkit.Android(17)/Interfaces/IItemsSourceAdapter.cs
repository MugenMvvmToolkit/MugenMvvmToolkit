using System.Collections;
using Android.Widget;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IItemsSourceAdapter : ISpinnerAdapter
    {
        /// <summary>
        ///     Gets or sets the items source.
        /// </summary>
        IEnumerable ItemsSource { get; set; }

        /// <summary>
        ///     Gets the position of item.
        /// </summary>
        int GetPosition(object value);

        /// <summary>
        ///     Gets the item from the specified position.
        /// </summary>
        object GetRawItem(int position);
    }
}