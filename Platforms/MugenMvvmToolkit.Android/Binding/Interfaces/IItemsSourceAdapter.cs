#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceAdapter.cs">
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

using System.Collections;
using Android.Widget;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
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