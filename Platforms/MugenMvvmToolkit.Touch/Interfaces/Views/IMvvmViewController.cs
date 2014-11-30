#region Copyright
// ****************************************************************************
// <copyright file="IMvvmViewController.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MonoTouch.Foundation;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IMvvmViewController
    {
        event EventHandler ViewDidLoadHandler;

        event EventHandler<ValueEventArgs<bool>> ViewWillAppearHandler;

        event EventHandler<ValueEventArgs<bool>> ViewDidAppearHandler;

        event EventHandler<ValueEventArgs<bool>> ViewDidDisappearHandler;

        event EventHandler<ValueEventArgs<bool>> ViewWillDisappearHandler;

        event EventHandler<ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        event EventHandler<ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        event EventHandler DisposeHandler;
    }
}