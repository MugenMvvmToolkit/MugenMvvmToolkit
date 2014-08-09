#region Copyright
// ****************************************************************************
// <copyright file="ToastPresenterBase.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Presenters;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.Foundation;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Documents;
#endif
#if WINDOWS_PHONE
using Microsoft.Phone.Controls;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    /// <summary>
    ///     Provides functionality to present a timed message.
    /// </summary>
    public class ToastPresenterBase : IToastPresenter
    {
        #region Nested types

        private sealed class EventClosure
        {
            #region Fields

            private readonly ToastPosition _position;
            private readonly WeakReference _reference;
#if NETFX_CORE || WINDOWSCOMMON
            private readonly Window _parent;
#else
            private readonly FrameworkElement _parent;
#endif
            public DispatcherTimer Timer;
            public TaskCompletionSource<object> TaskCompletionSource;
            public float Duration;
            public List<EventClosure> Closures;

            #endregion

            #region Constructors

#if NETFX_CORE || WINDOWSCOMMON
            public EventClosure(Popup popup, ToastPosition position, Window parent)
#else
            public EventClosure(Popup popup, ToastPosition position, FrameworkElement parent)
#endif

            {
                _position = position;
                _parent = parent;
                _reference = ServiceProvider.WeakReferenceFactory(popup, true);
            }

            #endregion

            #region Properties

            public Popup Popup
            {
                get { return (Popup)_reference.Target; }
            }

            #endregion

            #region Methods

            public void Handle(object sender, object args)
            {
                var popup = (Popup)_reference.Target;
                if (popup == null)
                    Clear();
                else
                    UpdatePosition(_parent, popup, _position);
            }

            public void Clear()
            {
                var popup = (Popup)_reference.Target;
                if (popup != null)
                    popup.IsOpen = false;
#if WPF
                ((Window)_parent).LocationChanged -= Handle;
#elif WINDOWS_PHONE
                var page = _parent as PhoneApplicationFrame;
                if (page != null)
                    page.OrientationChanged -= Handle;
#endif
                _parent.SizeChanged -= Handle;
                if (Timer != null)
                    Timer.Stop();
                if (TaskCompletionSource != null)
                    TaskCompletionSource.TrySetResult(null);
                Closures.Remove(this);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IThreadManager _threadManager;
#if WPF
        private static readonly Action<Popup> RepositionMethod;
#endif
        private const string PopupAttachedValuePath = "#%popups3w5";

        #endregion

        #region Constructors

#if WPF
        static ToastPresenterBase()
        {
            var method = typeof(Popup).GetMethod("Reposition", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
                RepositionMethod = (Action<Popup>)ServiceProvider.ReflectionManager.GetMethodDelegate(typeof(Action<Popup>), method);
        }
#endif

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPresenterBase" /> class.
        /// </summary>
        public ToastPresenterBase([NotNull]IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
            Background = new SolidColorBrush(Color.FromArgb(255, 105, 105, 105));
            Foreground = new SolidColorBrush(Color.FromArgb(255, 247, 247, 247));
#if WPF
            PopupAnimation = PopupAnimation.Slide;
#endif
        }

        #endregion

        #region Properties

        public Brush Background { get; set; }

        public Brush Foreground { get; set; }

        public FontFamily FontFamily { get; set; }

        public double? FontSize { get; set; }

#if WPF
        public PopupAnimation PopupAnimation { get; set; }
#endif
        #endregion

        #region Implementation of IToastPresenter

        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        public Task ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom, IDataContext context = null)
        {
            var tcs = new TaskCompletionSource<object>();
            if (_threadManager.IsUiThread)
                ShowInternal(content, duration, position, context, tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => ShowInternal(content, duration, position, context, tcs));
            return tcs.Task;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Shows the specified message.
        /// </summary>
        protected virtual void ShowInternal(object content, float duration, ToastPosition position, IDataContext context, TaskCompletionSource<object> tcs)
        {
#if WPF
            var placementTarget = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
#endif
#if SILVERLIGHT
            var placementTarget = Application.Current.RootVisual as FrameworkElement;
#endif
#if NETFX_CORE || WINDOWSCOMMON
            var placementTarget = Window.Current;
#endif

            if (placementTarget == null)
                return;
            var popups = ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(placementTarget, PopupAttachedValuePath, (window, o) => new List<EventClosure>(), null);
            for (int index = 0; index < popups.Count; index++)
            {
                var eventClosure = popups[index];
                if (duration >= eventClosure.Duration)
                {
                    eventClosure.Clear();
                    index--;
                }
            }

            var popup = GetToastPopup(GetToastContentWrapper(GetToastContent(content)));
#if WPF
            popup.PlacementTarget = placementTarget;
#endif
            UpdatePosition(placementTarget, popup, position);
            popup.IsOpen = true;
            var closure = new EventClosure(popup, position, placementTarget)
            {
                TaskCompletionSource = tcs,
                Duration = duration,
                Closures = popups
            };
            popups.Add(closure);
#if WPF
            placementTarget.LocationChanged += closure.Handle;
#elif WINDOWS_PHONE
            var page = placementTarget as PhoneApplicationFrame;
            if (page != null)
                page.OrientationChanged += closure.Handle;
#endif
            placementTarget.SizeChanged += closure.Handle;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(duration)
            };
            timer.Tick += (sender, args) => TimerCallback(sender, closure);
            closure.Timer = timer;
            BeginOpenAnimation(popup, timer.Start);
        }

        protected virtual Popup GetToastPopup(FrameworkElement content)
        {
            return new Popup
            {
                Visibility = Visibility.Visible,
#if WPF
                AllowsTransparency = true,
                Placement = PlacementMode.Relative,
                Child = new Border { Background = Brushes.Transparent, Child = content },
                PopupAnimation = PopupAnimation
#else
                Child = content
#endif
            };
        }

        protected virtual FrameworkElement GetToastContentWrapper(FrameworkElement content)
        {
            var border = new Border
            {
                Background = Background,
                Child = content,
                CornerRadius = new CornerRadius(2),
                VerticalAlignment = VerticalAlignment.Center
            };
#if !NETFX_CORE && !WINDOWS_PHONE && !WINDOWSCOMMON
            var colorBrush = Background as SolidColorBrush;
            if (colorBrush != null)
                border.Effect = new DropShadowEffect
                {
                    BlurRadius = 100,
                    Color = colorBrush.Color,
                    ShadowDepth = 0,
                    Opacity = 1
                };
#endif
            return border;
        }

        protected virtual FrameworkElement GetToastContent(object content)
        {
#if !WINDOWS_PHONE
            if (content != null)
            {
                var key = new DataTemplateKey(content.GetType());
#if NETFX_CORE || WINDOWSCOMMON
                if (Application.Current.Resources.ContainsKey(key))
#else
                if (Application.Current.Resources.Contains(key))
#endif
                {
                    var dataTemplate = Application.Current.Resources[key] as DataTemplate;
                    if (dataTemplate != null)
                    {
                        var element = dataTemplate.LoadContent() as FrameworkElement;
                        if (element != null)
                            element.DataContext = content;
                        return element;
                    }
                }
            }
#endif
            string msg = content == null ? "(null)" : content.ToString();
            var text = new TextBlock
            {
                Text = msg,
                Foreground = Foreground,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
#if WINDOWS_PHONE
            object style;
            if (Application.Current.Resources.Contains("PhoneTextNormalStyle"))
                text.Style = Application.Current.Resources["PhoneTextNormalStyle"] as Style;
#elif WINDOWSCOMMON
            object style;
            if (Application.Current.Resources.TryGetValue("BaseTextBlockStyle", out style))
                text.Style = style as Style;
            text.LineStackingStrategy = LineStackingStrategy.MaxHeight;
#elif NETFX_CORE
            object style;
            if (Application.Current.Resources.TryGetValue("BasicTextStyle", out style))
                text.Style = style as Style;
            text.LineStackingStrategy = LineStackingStrategy.MaxHeight;
#else
#if WPF
            text.TextTrimming = TextTrimming.CharacterEllipsis;
#endif
            text.LineHeight = 20;
            text.FontSize = 12;
            Typography.SetStylisticSet20(text, true);
            Typography.SetDiscretionaryLigatures(text, true);
            Typography.SetCaseSensitiveForms(text, true);
#endif
            if (FontSize.HasValue)
                text.FontSize = FontSize.Value;
            if (FontFamily != null)
                text.FontFamily = FontFamily;
            return text;
        }

        protected virtual void BeginOpenAnimation(Popup popup, Action completed)
        {
#if WPF
            completed();
#else
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform());
            transformGroup.Children.Add(new RotateTransform());
            transformGroup.Children.Add(new ScaleTransform
            {
                CenterX = popup.HorizontalOffset,
                CenterY = popup.VerticalOffset
            });
            if (popup.RenderTransform != null)
                transformGroup.Children.Add(popup.RenderTransform);
            popup.RenderTransform = transformGroup;

            var sb = new Storyboard();
            var translateAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0),
            };
            translateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, -8));
            translateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 0));


            var rotateAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0),
            };
            rotateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.1, -0.1));
            rotateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 0));

            var scaleAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0)
            };
            scaleAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, 0.6));
            scaleAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 1));

            var opacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0)
            };
            opacityAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, 0));
            opacityAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 1));

            sb.Children.Add(translateAnimation);
            sb.Children.Add(rotateAnimation);
            sb.Children.Add(scaleAnimation);
            sb.Children.Add(opacityAnimation);

            Storyboard.SetTarget(translateAnimation, popup);
            Storyboard.SetTarget(rotateAnimation, popup);
            Storyboard.SetTarget(scaleAnimation, popup);
            Storyboard.SetTarget(opacityAnimation, popup.Child);

            SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)");
            SetTargetProperty(rotateAnimation, "(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)");
            SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleY)");
            SetTargetProperty(opacityAnimation, "Opacity");
            sb.Completed += (sender, args) => completed();
            sb.Begin();
#endif
        }

        protected virtual void BeginCloseAnimation(Popup popup, Action completed)
        {
#if WPF
            completed();
#else
            var sb = new Storyboard();
            var translateAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0),
            };
            translateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, 0));
            translateAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 50));

            var scaleAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0)
            };
            scaleAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, 1));
            scaleAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 0.6));

            var opacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = new TimeSpan(0)
            };
            opacityAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0, 1));
            opacityAnimation.KeyFrames.Add(GetSplineDoubleKeyFrame(0.3, 0));

            sb.Children.Add(translateAnimation);
            sb.Children.Add(scaleAnimation);
            sb.Children.Add(opacityAnimation);

            Storyboard.SetTarget(translateAnimation, popup);
            Storyboard.SetTarget(scaleAnimation, popup);
            if (popup.Child != null)
                Storyboard.SetTarget(opacityAnimation, popup.Child);

            SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)");
            SetTargetProperty(scaleAnimation, "(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleY)");
            SetTargetProperty(opacityAnimation, "Opacity");
            sb.Completed += (sender, args) => completed();
            sb.Begin();
#endif
        }

        private void TimerCallback(object sender, EventClosure closure)
        {
            ((DispatcherTimer)sender).Stop();
            var popup = closure.Popup;
            if (popup != null)
                BeginCloseAnimation(popup, closure.Clear);
        }

#if NETFX_CORE || WINDOWSCOMMON
        private static void UpdatePosition(Window parent, Popup popup, ToastPosition position)
#else
        private static void UpdatePosition(FrameworkElement parent, Popup popup, ToastPosition position)
#endif
        {
            var control = (FrameworkElement)popup.Child;
#if NETFX_CORE || WINDOWSCOMMON
            double parentWidth = parent.Bounds.Width;
            double parentHeight = parent.Bounds.Height;
#elif WINDOWS_PHONE
            double parentWidth = parent.ActualWidth;
            double parentHeight = parent.ActualHeight;
            bool isLandscape = false;
            var frame = parent as PhoneApplicationFrame;
            if (frame != null)
            {
                isLandscape = frame.Orientation == PageOrientation.LandscapeLeft ||
                              frame.Orientation == PageOrientation.LandscapeRight;
                if (isLandscape)
                {
                    if (frame.Orientation == PageOrientation.LandscapeLeft)
                    {
                        if (position == ToastPosition.Bottom)
                            position = ToastPosition.Top;
                        else if (position == ToastPosition.Top)
                            position = ToastPosition.Bottom;
                    }
                    parentWidth = parent.ActualHeight;
                    parentHeight = parent.ActualWidth;
                    if (popup.Child != null)
                    {
                        popup.Child.RenderTransformOrigin = new Point(0.5, 0.5);
                        popup.Child.RenderTransform = new RotateTransform
                        {
                            Angle = frame.Orientation == PageOrientation.LandscapeLeft ? 90 : -90
                        };
                    }
                }
                else if (popup.Child != null)
                {
                    popup.Child.RenderTransformOrigin = new Point(0, 0);
                    popup.Child.RenderTransform = null;
                }
            }
#else
            double parentWidth = parent.ActualWidth;
            double parentHeight = parent.ActualHeight;
#endif
            control.MaxWidth = parentWidth - 60;
            control.MaxHeight = parentHeight;
            control.Measure(new Size(parentWidth, parentHeight));

#if WINDOWS_PHONE
            if (isLandscape)
                popup.VerticalOffset = (parentWidth - control.DesiredSize.Width) / 2;
            else
                popup.HorizontalOffset = (parentWidth - control.DesiredSize.Width) / 2;
#elif WPF
            popup.HorizontalOffset = ((parentWidth - control.DesiredSize.Width) / 2) - 8;
#else
            popup.HorizontalOffset = (parentWidth - control.DesiredSize.Width) / 2;
#endif
            double verticalOffset;
            switch (position)
            {
                case ToastPosition.Bottom:
#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
                    verticalOffset = parentHeight - control.DesiredSize.Height - 85;
#else
                    verticalOffset = parentHeight - control.DesiredSize.Height - 50;
#endif
                    break;
                case ToastPosition.Center:
                    verticalOffset = (parentHeight - control.DesiredSize.Height) / 2;
                    break;
                case ToastPosition.Top:
                    verticalOffset = 50;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("position");
            }

#if WINDOWS_PHONE
            if (isLandscape)
                popup.HorizontalOffset = verticalOffset;
            else
                popup.VerticalOffset = verticalOffset;
#else
            popup.VerticalOffset = verticalOffset;
#endif

#if WPF
            if (popup.IsOpen && RepositionMethod != null)
                RepositionMethod(popup);
#endif
        }

#if !WPF
        private static void SetTargetProperty(Timeline target, string property)
        {
#if NETFX_CORE || WINDOWSCOMMON
            Storyboard.SetTargetProperty(target, property);
#else
            Storyboard.SetTargetProperty(target, new PropertyPath(property));
#endif
        }

        private static SplineDoubleKeyFrame GetSplineDoubleKeyFrame(double seconds, double value)
        {
            return new SplineDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds)),
                Value = value
            };
        }
#endif
        #endregion
    }
}