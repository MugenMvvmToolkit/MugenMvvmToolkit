#region Copyright

// ****************************************************************************
// <copyright file="MvvmViewControllerMediator.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Mediators
{
    public class MvvmViewControllerMediator : IMvvmViewControllerMediator
    {
        #region Fields

        private WeakReference _viewController;
        private bool _isAppeared;
        private bool _canDispose;
        private bool _isDisposeCalled;
        private bool _isViewLoaded;

        #endregion

        #region Constructors

        public MvvmViewControllerMediator([NotNull] UIViewController viewController)
        {
            Should.NotBeNull(viewController, nameof(viewController));
            _viewController = ServiceProvider.WeakReferenceFactory(viewController);
            _canDispose = true;
            var viewModel = viewController.DataContext() as IViewModel;
            if (viewModel == null || !viewModel.Settings.Metadata.Contains(ViewModelConstants.StateNotNeeded))
                viewController.InititalizeRestorationIdentifier();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public UIViewController ViewController => (UIViewController)_viewController?.Target;

        #endregion

        #region Implementation of IMvvmViewControllerMediator

        public bool IsDisappeared => _canDispose;

        public bool IsViewLoaded => _isViewLoaded;

        public bool IsAppeared => !_canDispose;

        public virtual void ViewWillAppear(Action<bool> baseViewWillAppear, bool animated)
        {
            baseViewWillAppear(animated);

            var viewController = ViewController;
            if (viewController != null && !_isAppeared)
            {
                TouchToolkitExtensions.NativeObjectManager?.Initialize(viewController, null);
                _isAppeared = true;
            }
            Raise(ViewWillAppearHandler, animated);
        }

        public virtual void ViewDidAppear(Action<bool> baseViewDidAppear, bool animated)
        {
            baseViewDidAppear(animated);
            Raise(ViewDidAppearHandler, animated);
            _canDispose = false;
        }

        public virtual void ViewDidDisappear(Action<bool> baseViewDidDisappear, bool animated)
        {
            baseViewDidDisappear(animated);
            Raise(ViewDidDisappearHandler, animated);
            _canDispose = true;
            TryDispose();
        }

        public virtual void ViewDidLoad(Action baseViewDidLoad)
        {
            baseViewDidLoad();
            _isViewLoaded = true;
            Raise(ViewDidLoadHandler);
        }

        public virtual void ViewWillDisappear(Action<bool> baseViewWillDisappear, bool animated)
        {
            baseViewWillDisappear(animated);
            Raise(ViewWillDisappearHandler, animated);
        }

        public virtual void DecodeRestorableState(Action<NSCoder> baseDecodeRestorableState, NSCoder coder)
        {
            baseDecodeRestorableState(coder);
            var viewController = ViewController;
            if (viewController != null)
                TouchToolkitExtensions.ApplicationStateManager.DecodeState(viewController, coder);
            Raise(DecodeRestorableStateHandler, coder);
        }

        public virtual void EncodeRestorableState(Action<NSCoder> baseEncodeRestorableState, NSCoder coder)
        {
            baseEncodeRestorableState(coder);
            var viewController = ViewController;
            if (viewController != null)
                TouchToolkitExtensions.ApplicationStateManager.EncodeState(viewController, coder);
            Raise(EncodeRestorableStateHandler, coder);
        }

        public virtual void Dispose(Action<bool> baseDispose, bool disposing)
        {
            if (disposing)
            {
                if (!_isDisposeCalled)
                {
                    _isDisposeCalled = true;
                    TryDispose();
                    return;
                }
            }
            baseDispose(disposing);
        }

        public virtual event EventHandler<UIViewController, EventArgs> ViewDidLoadHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<NSCoder>> EncodeRestorableStateHandler;

        public virtual event EventHandler<UIViewController, EventArgs> DisposeHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillAppearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidAppearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewDidDisappearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<bool>> ViewWillDisappearHandler;

        public virtual event EventHandler<UIViewController, ValueEventArgs<NSCoder>> DecodeRestorableStateHandler;

        #endregion

        #region Methods

        private void TryDispose()
        {
            var viewController = ViewController;
            if (viewController == null)
                return;
            if (!_canDispose || !_isDisposeCalled)
                return;
            Raise(DisposeHandler);
            viewController.ClearBindings(true, true);
            viewController.DisposeEx();
            ViewDidLoadHandler = null;
            ViewWillAppearHandler = null;
            ViewDidAppearHandler = null;
            ViewDidDisappearHandler = null;
            ViewWillDisappearHandler = null;
            DecodeRestorableStateHandler = null;
            EncodeRestorableStateHandler = null;
            DisposeHandler = null;
            _viewController = null;
        }

        private void Raise(EventHandler<UIViewController, EventArgs> handler)
        {
            var viewController = ViewController;
            if (viewController != null)
                handler?.Invoke(viewController, EventArgs.Empty);
        }

        private void Raise<T>(EventHandler<UIViewController, ValueEventArgs<T>> handler, T value)
        {
            var viewController = ViewController;
            if (viewController != null)
                handler?.Invoke(viewController, new ValueEventArgs<T>(value));
        }

        #endregion
    }
}
