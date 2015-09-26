#region Copyright

// ****************************************************************************
// <copyright file="IFragmentView.cs">
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

using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

#if APPCOMPAT
using Fragment = Android.Support.V4.App.Fragment;
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Mediators;

namespace MugenMvvmToolkit.Android.AppCompat.Interfaces.Views
#else
using Android.App;
using MugenMvvmToolkit.Android.Interfaces.Mediators;

namespace MugenMvvmToolkit.Android.Interfaces.Views
#endif
{
    public interface IFragmentView : IView
    {
        [NotNull]
        IMvvmFragmentMediator Mediator { get; }

        object DataContext { get; set; }

        event EventHandler<Fragment, EventArgs> DataContextChanged;
    }
}
