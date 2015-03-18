#region Copyright

// ****************************************************************************
// <copyright file="PlatformWrapperRegistrationModule.cs">
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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    public class PlatformWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        private sealed class PopupWrapper : IDisposable, IWindowView, IViewWrapper
        {
            #region Fields

            private readonly IPopupView _popupView;
            private WindowSizeChangedEventHandler _weakSizeListener;
            private Popup _popup;

            #endregion

            #region Constructors

            public PopupWrapper(IPopupView popupView)
            {
                _popupView = popupView;
            }

            #endregion

            #region Implementation of IWindowView

            public void Dispose()
            {
                if (_popup != null)
                    _popup.Closed -= PopupOnClosed;
                if (_weakSizeListener != null)
                    Window.Current.SizeChanged -= _weakSizeListener;
                _popup = null;
                _weakSizeListener = null;
            }

            public void Show()
            {
                var bounds = Window.Current.Bounds;
                var child = (FrameworkElement)_popupView;
                _popup = new Popup
                {
                    Width = bounds.Width,
                    Height = bounds.Height,
                    Child = child,
                    IsLightDismissEnabled = false,
                };
                if (double.IsNaN(child.Width))
                    child.Width = bounds.Width;
                if (double.IsNaN(child.Height))
                    child.Height = bounds.Height;
                child.Measure(new Size(bounds.Width, bounds.Height));

                _popup.VerticalOffset = (bounds.Height - child.DesiredSize.Height) / 2;
                _popup.HorizontalOffset = (bounds.Width - child.DesiredSize.Width) / 2;
                bool trackSize = true;
                _popupView.InitializePopup(_popup, ref trackSize);
                if (trackSize)
                {
                    _weakSizeListener = ReflectionExtensions
                        .CreateWeakDelegate<PopupWrapper, WindowSizeChangedEventArgs, WindowSizeChangedEventHandler>(
                            this, (wrapper, o, arg3) => wrapper.OnWindowSizeChanged(arg3),
                            (o, handler) => ((Window)o).SizeChanged -= handler, handler => handler.Handle);
                    Window.Current.SizeChanged += _weakSizeListener;
                }
                _popup.Closed += PopupOnClosed;
                _popup.IsOpen = true;
            }

            public void ShowDialog()
            {
                Show();
            }

            public void Close()
            {
                if (_popup != null)
                    _popup.IsOpen = false;
            }

            public event EventHandler<object, CancelEventArgs> Closing
            {
                add { }
                remove { }
            }

            public event EventHandler<object, EventArgs> Closed;

            public object View
            {
                get { return _popupView; }
            }

            #endregion

            #region Methods

            private void PopupOnClosed(object sender, object o)
            {
                var closed = Closed;
                if (closed != null)
                    closed(this, EventArgs.Empty);
                Dispose();
            }

            private void OnWindowSizeChanged(WindowSizeChangedEventArgs args)
            {
                if (!_popup.IsOpen || _popup.Child == null)
                    return;
                var size = args.Size;
                var child = (FrameworkElement)_popup.Child;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (child.Width == _popup.Width)
                    child.Width = size.Width;
                if (child.Height == _popup.Height)
                    child.Height = size.Height;
                // ReSharper restore CompareOfFloatsByEqualityOperator
                _popup.Width = size.Width;
                _popup.Height = size.Height;
                child.Measure(new Size(size.Width, size.Height));
                _popup.VerticalOffset = (size.Height - child.DesiredSize.Height) / 2;
                _popup.HorizontalOffset = (size.Width - child.DesiredSize.Width) / 2;
            }

            #endregion
        }

        private sealed class SettingsFlyoutWrapper : IDisposable, IWindowView, IViewWrapper
        {
            #region Fields

            private readonly SettingsFlyout _flyout;

            #endregion

            #region Constructors

            public SettingsFlyoutWrapper(SettingsFlyout flyout)
            {
                _flyout = flyout;
                _flyout.BackClick += FlyoutOnBackClick;
                _flyout.Unloaded += FlyoutOnUnloaded;
            }

            #endregion

            #region Implementation of IWindowView

            public void Dispose()
            {
                _flyout.BackClick -= FlyoutOnBackClick;
                _flyout.Unloaded -= FlyoutOnUnloaded;
            }

            public void Show()
            {
                _flyout.Show();
            }

            public void ShowDialog()
            {
                _flyout.ShowIndependent();
            }

            public void Close()
            {
                _flyout.Hide();
            }

            public event EventHandler<object, CancelEventArgs> Closing;

            public event EventHandler<object, EventArgs> Closed;

            public object View
            {
                get { return _flyout; }
            }

            #endregion

            #region Methods

            private void FlyoutOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                var closed = Closed;
                if (closed != null)
                    closed(this, EventArgs.Empty);
                Dispose();
            }

            private void FlyoutOnBackClick(object sender, BackClickEventArgs backClickEventArgs)
            {
                var closing = Closing;
                if (closing != null)
                {
                    var args = new CancelEventArgs();
                    closing(this, args);
                    backClickEventArgs.Handled = args.Cancel;
                }
            }

            #endregion
        }

        private sealed class ContentDialogWrapper : IDisposable, IWindowView, IViewWrapper
        {
            #region Fields

            private static readonly MethodInfo OnClosingMethod;
            private static EventInfo _closingEvent;
            private static PropertyInfo _cancelProperty;
            private static MethodInfo _showAsyncMethod;
            private static MethodInfo _hideMethod;

            private readonly EventRegistrationToken? _token;
            private readonly FrameworkElement _window;

            #endregion

            #region Constructors

            static ContentDialogWrapper()
            {
                OnClosingMethod = typeof(ContentDialogWrapper).GetMethodEx("OnClosing",
                    MemberFlags.Instance | MemberFlags.NonPublic);
            }

            public ContentDialogWrapper(FrameworkElement window)
            {
                _window = window;
                if (_closingEvent == null)
                    _closingEvent = window.GetType().GetRuntimeEvent("Closing");

                Delegate handler = ServiceProvider
                    .ReflectionManager
                    .TryCreateDelegate(_closingEvent.EventHandlerType, this, OnClosingMethod);
                if (handler == null)
                {
                    Tracer.Error("The provider cannot create delegate for event '{0}'", _closingEvent.EventHandlerType);
                    return;
                }
                _token = (EventRegistrationToken)_closingEvent.AddMethod.InvokeEx(window, handler);
            }

            #endregion

            #region Methods

            [UsedImplicitly]
            private void OnClosing(object sender, object args)
            {
                EventHandler<object, CancelEventArgs> handler = Closing;
                if (handler == null)
                    return;
                var eventArgs = new CancelEventArgs();
                handler(this, eventArgs);
                if (_cancelProperty == null)
                    _cancelProperty = args.GetType().GetPropertyEx("Cancel", MemberFlags.Instance | MemberFlags.Public);
                if (_cancelProperty != null)
                    _cancelProperty.SetValueEx(args, eventArgs.Cancel);
            }

            #endregion

            #region Implementation of IWindowView

            public void Show()
            {
                if (_showAsyncMethod == null)
                    _showAsyncMethod = _window.GetType()
                        .GetMethodEx("ShowAsync", MemberFlags.Instance | MemberFlags.Public);
                if (_showAsyncMethod != null)
                    _showAsyncMethod.InvokeEx(_window);
            }

            public void ShowDialog()
            {
                Show();
            }

            public void Close()
            {
                if (_hideMethod == null)
                    _hideMethod = _window.GetType().GetMethodEx("Hide", MemberFlags.Instance | MemberFlags.Public);
                if (_hideMethod != null)
                    _hideMethod.InvokeEx(_window);
            }

            public event EventHandler<object, CancelEventArgs> Closing;

            public event EventHandler<object, EventArgs> Closed
            {
                add { }
                remove { }
            }

            #endregion

            #region Implementation of IViewWrapper

            public object View
            {
                get { return _window; }
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (_token.HasValue)
                    _closingEvent.RemoveMethod.Invoke(_window, new object[] { _token.Value });
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ContentDialogFullName = "Windows.UI.Xaml.Controls.ContentDialog";

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        /// <summary>
        ///     Registers the wrappers using <see cref="WrapperManager" /> class.
        /// </summary>
        protected override void RegisterWrappers(WrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, ContentDialogWrapper>(IsContentDialog,
                (o, context) => new ContentDialogWrapper((FrameworkElement)o));
            wrapperManager.AddWrapper<IWindowView, SettingsFlyoutWrapper>(
                (type, context) => typeof(SettingsFlyout).IsAssignableFrom(type),
                (o, context) => new SettingsFlyoutWrapper((SettingsFlyout)o));
            wrapperManager.AddWrapper<IWindowView, PopupWrapper>(
                (type, context) => typeof(IPopupView).IsAssignableFrom(type),
                (o, context) => new PopupWrapper((IPopupView)o));
        }

        #endregion

        #region Methods

        private static bool IsContentDialog(Type type, IDataContext context)
        {
            while (typeof(object) != type)
            {
                if (type.FullName == ContentDialogFullName)
                    return true;
                type = type.GetTypeInfo().BaseType;
            }
            return false;
        }

        #endregion
    }
}