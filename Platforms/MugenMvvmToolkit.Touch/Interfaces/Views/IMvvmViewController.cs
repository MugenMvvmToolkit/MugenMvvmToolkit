using System;
using MonoTouch.Foundation;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Views
{
    public interface IMvvmViewController : IView
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