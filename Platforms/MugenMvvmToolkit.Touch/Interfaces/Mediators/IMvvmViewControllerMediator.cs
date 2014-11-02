using System;
using JetBrains.Annotations;
using MonoTouch.Foundation;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Mediators
{
    public interface IMvvmViewControllerMediator
    {
        void ViewWillAppear([NotNull] Action<bool> baseViewWillAppear, bool animated);

        void ViewDidAppear([NotNull] Action<bool> baseViewDidAppear, bool animated);

        void ViewDidDisappear([NotNull] Action<bool> baseViewDidDisappear, bool animated);

        void ViewDidLoad([NotNull] Action baseViewDidLoad);

        void ViewWillDisappear([NotNull] Action<bool> baseViewWillDisappear, bool animated);

        void DecodeRestorableState([NotNull] Action<NSCoder> baseDecodeRestorableState, NSCoder coder);

        void EncodeRestorableState([NotNull] Action<NSCoder> baseEncodeRestorableState, NSCoder coder);

        void Dispose([NotNull] Action<bool> baseDispose, bool disposing);

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