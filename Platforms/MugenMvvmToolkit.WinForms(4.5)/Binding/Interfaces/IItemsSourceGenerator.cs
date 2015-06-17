#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceGenerator.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

#if ANDROID
namespace MugenMvvmToolkit.Android.Binding.Interfaces
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Binding.Interfaces
#elif WINFORMS
namespace MugenMvvmToolkit.WinForms.Binding.Interfaces
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Interfaces
#endif
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

#if ANDROID
    /// <summary>
    ///     Represents the interface that allows to generate items from collection.
    /// </summary>
    public interface IItemsSourceGeneratorEx : IItemsSourceGenerator
    {
        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        object SelectedItem { get; set; }
    }
#endif

}