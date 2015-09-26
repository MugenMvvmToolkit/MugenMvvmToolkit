#region Copyright

// ****************************************************************************
// <copyright file="IMvvmViewControllerMediator.cs">
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
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using UIKit;

namespace MugenMvvmToolkit.iOS.Interfaces.Mediators
{
    public interface IMvvmViewControllerMediator
    {
        bool IsDisappeared { get; }

        bool IsAppeared { get; }

        void ViewWillAppear([NotNull] Action<bool> baseViewWillAppear, bool animated);

        void ViewDidAppear([NotNull] Action<bool> baseViewDidAppear, bool animated);

        void ViewDidDisappear([NotNull] Action<bool> baseViewDidDisappear, bool animated);

        void ViewDidLoad([NotNull] Action baseViewDidLoad);

        void ViewWillDisappear([NotNull] Action<bool> baseViewWillDisappear, bool animated);

        void DecodeRestorableState([NotNull] Action<NSCoder> baseDecodeRestorableState, NSCoder coder);

        void EncodeRestorableState([NotNull] Action<NSCoder> baseEncodeRestorableState, NSCoder coder);

        void Dispose([NotNull] Action<bool> baseDispose, bool disposing);

        event EventHandler<UIViewController, EventArgs> ViewDidLoadHandler;

        event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillAppearHandler;

        event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidAppearHandler;

        event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidDisappearHandler;

        event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillDisappearHandler;

        event EventHandler<UIViewController, ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        event EventHandler<UIViewController, ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        event EventHandler<UIViewController, EventArgs> DisposeHandler;
    }
}
