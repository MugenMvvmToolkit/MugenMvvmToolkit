#region Copyright

// ****************************************************************************
// <copyright file="UwpWrapperRegistrationModule.cs">
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
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.UWP.Interfaces.Views;
using MugenMvvmToolkit.UWP.Models;

namespace MugenMvvmToolkit.UWP.Modules
{
    public class UwpWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        private sealed class PopupWrapper : IDisposable, IWindowView, IViewWrapper
        {
            #region Fields

            private static readonly Action<Popup, Size> UpdatePositionDelegate;
            private static readonly Action<Popup, Size> UpdateSizeDelegate;

            private readonly IPopupView _popupView;
            private double _flyoutOffset;
            private Popup _popup;
            private PopupSettings _settings;
            private TypedEventHandler<InputPane, InputPaneVisibilityEventArgs> _weakInputPaneListener;
            private WindowSizeChangedEventHandler _weakSizeListener;

            #endregion

            #region Constructors

            static PopupWrapper()
            {
                UpdatePositionDelegate = UpdatePosition;
                UpdateSizeDelegate = UpdateSize;
            }

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
                if (_weakInputPaneListener != null)
                {
                    InputPane pane = InputPane.GetForCurrentView();
                    if (pane != null)
                    {
                        pane.Hiding -= _weakInputPaneListener;
                        pane.Showing -= _weakInputPaneListener;
                    }
                }
                _popup = null;
                _weakSizeListener = null;
                _weakInputPaneListener = null;
                _settings = null;
            }

            public object View => _popupView;

            public void Show()
            {
                Rect bounds = Window.Current.Bounds;
                var child = (FrameworkElement)_popupView;
                _popup = new Popup
                {
                    Width = bounds.Width,
                    Height = bounds.Height,
                    Child = child,
                    IsLightDismissEnabled = false,
                    ChildTransitions =
                        new TransitionCollection
                        {
                            new PopupThemeTransition {FromHorizontalOffset = 0, FromVerticalOffset = 100}
                        }
                };
                InputPane inputPane = InputPane.GetForCurrentView();
                if (inputPane != null)
                    _flyoutOffset = inputPane.OccludedRect.Height;
                _settings = new PopupSettings
                {
                    UpdatePositionAction = UpdatePositionDelegate,
                    UpdateSizeAction = UpdateSizeDelegate
                };
                _popupView.InitializePopup(_popup, _settings);
                UpdatePopup(bounds.Width, bounds.Height);
                _popup.Closed += PopupOnClosed;
                if (_settings.ShowAction == null)
                    _popup.IsOpen = true;
                else
                    _settings.ShowAction(_popup);
                _weakSizeListener = ReflectionExtensions
                    .CreateWeakDelegate<PopupWrapper, WindowSizeChangedEventArgs, WindowSizeChangedEventHandler>(
                        this, (wrapper, o, arg3) => wrapper.OnWindowSizeChanged(arg3), (o, handler) => ((Window)o).SizeChanged -= handler, handler => handler.Handle);
                Window.Current.SizeChanged += _weakSizeListener;
                if (inputPane != null)
                {
                    _weakInputPaneListener = ReflectionExtensions
                        .CreateWeakDelegate<PopupWrapper, InputPaneVisibilityEventArgs,
                            TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(this,
                                (wrapper, o, arg3) => wrapper.OnInputPaneChanged(arg3),
                                (o, handler) =>
                                {
                                    var pane = (InputPane)o;
                                    pane.Hiding -= handler;
                                    pane.Showing -= handler;

                                }, handler => handler.Handle);
                    inputPane.Showing += _weakInputPaneListener;
                    inputPane.Hiding += _weakInputPaneListener;
                }
            }

            public void ShowDialog()
            {
                Show();
            }

            public void Close()
            {
                if (_popup != null)
                {
                    if (_settings == null || _settings.CloseAction == null)
                        _popup.IsOpen = false;
                    else
                        _settings.CloseAction(_popup);
                }
            }

            public bool Activate()
            {
                return false;
            }

            public event EventHandler<object, CancelEventArgs> Closing
            {
                add { }
                remove { }
            }

            public event EventHandler<object, EventArgs> Closed;

            #endregion

            #region Methods

            private static void UpdatePosition(Popup popup, Size size)
            {
                var child = (FrameworkElement)popup.Child;
                child.Measure(size);
                popup.VerticalOffset = (size.Height - child.DesiredSize.Height) / 2;
                popup.HorizontalOffset = (size.Width - child.DesiredSize.Width) / 2;
            }

            private static void UpdateSize(Popup popup, Size size)
            {
                var child = (FrameworkElement)popup.Child;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (child.Width == popup.Width || double.IsNaN(child.Width))
                    child.Width = size.Width;
                if (child.Height == popup.Height || double.IsNaN(child.Height))
                    child.Height = size.Height;
                // ReSharper restore CompareOfFloatsByEqualityOperator
                popup.Width = size.Width;
                popup.Height = size.Height;
            }

            private void PopupOnClosed(object sender, object o)
            {
                Closed?.Invoke(this, EventArgs.Empty);
                Dispose();
            }

            private void OnWindowSizeChanged(WindowSizeChangedEventArgs args)
            {
                if (_popup.IsOpen && _popup.Child != null && _settings != null)
                    UpdatePopup(args.Size.Width, args.Size.Height);
            }

            private void OnInputPaneChanged(InputPaneVisibilityEventArgs args)
            {
                _flyoutOffset = args.OccludedRect.Height;
                if (_popup.IsOpen && _popup.Child != null && _settings != null)
                {
                    var bounds = Window.Current.Bounds;
                    UpdatePopup(bounds.Width, bounds.Height);
                }
            }

            private void UpdatePopup(double width, double height)
            {
                var size = new Size(width, Math.Max(height - _flyoutOffset, 1));
                _settings.UpdateSizeAction?.Invoke(_popup, size);
                _settings.UpdatePositionAction?.Invoke(_popup, size);
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

            public object View => _flyout;

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

            public bool Activate()
            {
                return _flyout.Focus(FocusState.Programmatic);
            }

            public event EventHandler<object, CancelEventArgs> Closing;

            public event EventHandler<object, EventArgs> Closed;

            #endregion

            #region Methods

            private void FlyoutOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                Closed?.Invoke(this, EventArgs.Empty);
                Dispose();
            }

            private void FlyoutOnBackClick(object sender, BackClickEventArgs backClickEventArgs)
            {
                EventHandler<object, CancelEventArgs> closing = Closing;
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

            private readonly ContentDialog _window;

            #endregion

            #region Constructors

            public ContentDialogWrapper(ContentDialog window)
            {
                _window = window;
                _window.Closing += OnClosing;
                _window.Closed += OnClosed;
            }

            #endregion

            #region Methods

            private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
            {
                EventHandler<object, CancelEventArgs> handler = Closing;
                if (handler == null)
                    return;
                var eventArgs = new CancelEventArgs();
                handler(this, eventArgs);
                args.Cancel = args.Cancel;
            }

            private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }

            #endregion

            #region Implementation of IWindowView

            public void Show()
            {
                _window.ShowAsync();
            }

            public void ShowDialog()
            {
                Show();
            }

            public void Close()
            {
                _window.Hide();
            }

            public bool Activate()
            {
                return _window.Focus(FocusState.Programmatic);
            }

            public event EventHandler<object, CancelEventArgs> Closing;

            public event EventHandler<object, EventArgs> Closed;

            #endregion

            #region Implementation of IViewWrapper

            public object View => _window;

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                _window.Closing -= OnClosing;
                _window.Closed -= OnClosed;
            }

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        protected override void RegisterWrappers(IConfigurableWrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, ContentDialogWrapper>((type, context) => typeof(ContentDialog).IsAssignableFrom(type),
                (o, context) => new ContentDialogWrapper((ContentDialog)o));
            wrapperManager.AddWrapper<IWindowView, SettingsFlyoutWrapper>(
                (type, context) => typeof(SettingsFlyout).IsAssignableFrom(type),
                (o, context) => new SettingsFlyoutWrapper((SettingsFlyout)o));
            wrapperManager.AddWrapper<IWindowView, PopupWrapper>(
                (type, context) => typeof(IPopupView).IsAssignableFrom(type),
                (o, context) => new PopupWrapper((IPopupView)o));
        }

        #endregion
    }
}
