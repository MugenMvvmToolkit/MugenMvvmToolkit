using System;
using Android.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using Fragment = Android.Support.V4.App.Fragment;
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;

namespace MugenMvvmToolkit.AppCompat.Interfaces.Views
#else
using MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators;

namespace MugenMvvmToolkit.FragmentSupport.Interfaces.Views
#endif
{
    public interface IFragmentView : IView
    {
        /// <summary>
        ///     Gets the current <see cref="IMvvmFragmentMediator" />.
        /// </summary>
        [NotNull]
        IMvvmFragmentMediator Mediator { get; }

        /// <summary>
        ///     Gets or sets the data context of the current view.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Fragment, EventArgs> DataContextChanged;
    }
}