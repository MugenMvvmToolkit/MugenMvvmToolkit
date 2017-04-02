#region Copyright

// ****************************************************************************
// <copyright file="ToastView.cs">
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
using System.Threading.Tasks;
using CoreGraphics;
using MugenMvvmToolkit.Models;
using UIKit;

#if XAMARIN_FORMS
using MugenMvvmToolkit.Xamarin.Forms.iOS.Interfaces;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Views
#else
using MugenMvvmToolkit.iOS.Interfaces;

namespace MugenMvvmToolkit.iOS.Views
#endif

{
    public class ToastView : IDisposable, IOrientationChangeListener
    {
        #region Fields

        private UILabel _label;

        #endregion

        #region Constructors

        protected ToastView()
        {
        }

        public ToastView(object content, UIView owner)
        {
            Content = content;
            Owner = owner ?? UIApplication.SharedApplication.KeyWindow;
            AnimationDuration = 0.2;
            ShadowEnabled = true;
            ShadowOpacity = 0.8f;
            ShadowOffsetX = 4f;
            ShadowOffsetY = 4f;
            ShadowRadius = 4f;
            HorizontalPadding = 10f;
            VerticalPadding = 10f;
            FontSize = 16f;
            CornerRadius = 4f;
            if (PlatformExtensions.IsOS7)
            {
                BackgroundColor = UIColor.Black.ColorWithAlpha(0.8f);
                ShadowColor = UIColor.Black.CGColor;
                TextColor = UIColor.White;
            }
            else
            {
                BackgroundColor = UIColor.White.ColorWithAlpha(0.8f);
                ShadowColor = UIColor.White.CGColor;
                TextColor = UIColor.Black;
            }
        }

        #endregion

        #region Properties

        public double AnimationDuration { get; set; }

        public bool ShadowEnabled { get; set; }

        public float ShadowOpacity { get; set; }

        public nfloat ShadowOffsetX { get; set; }

        public nfloat ShadowOffsetY { get; set; }

        public nfloat ShadowRadius { get; set; }

        public nfloat VerticalPadding { get; set; }

        public nfloat HorizontalPadding { get; set; }

        public CGPoint? CustomPosition { get; set; }

        public int MaxMessageLines { get; set; }

        public nfloat FontSize { get; set; }

        public nfloat CornerRadius { get; set; }

        public UIColor BackgroundColor { get; set; }

        public UIColor TextColor { get; set; }

        public CGColor ShadowColor { get; set; }

        public UIView Owner { get; protected set; }

        public nfloat DisplayDuration { get; set; }

        public ToastPosition Position { get; set; }

        public TaskCompletionSource<object> TaskCompletionSource { get; set; }

        protected object Content { get; set; }

        protected UIView View { get; set; }

        #endregion

        #region Methods

        public virtual void Show()
        {
            if (View != null)
                return;

            View = CreateToastView();
            View.Center = GetCenterPoint(View);
            View.Alpha = 0f;
            View.Layer.ZPosition = float.MaxValue;
            PlatformExtensions.AddOrientationChangeListener(this);

            if (Owner is UIWindow && !PlatformExtensions.IsOS8)
                UpdateWindowOrientation(View);

            Owner.AddSubview(View);
            Owner.BringSubviewToFront(View);
            UIView.Animate(AnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseOut, () => View.Alpha = 1f,
                ShowCompleted);
        }

        public virtual void Hide()
        {
            Dispose();
        }

        protected virtual UIView CreateToastView()
        {
            if (Content == null)
                Content = "(null)";

            var uiView = new UIView
            {
                AutoresizingMask = UIViewAutoresizing.None,
                AutosizesSubviews = false,
                BackgroundColor = BackgroundColor
            };
            uiView.Layer.CornerRadius = CornerRadius;
            if (ShadowEnabled)
            {
                uiView.Layer.ShadowColor = ShadowColor;
                uiView.Layer.ShadowOpacity = ShadowOpacity;
                uiView.Layer.ShadowOffset = new CGSize(ShadowOffsetX, ShadowOffsetY);
                uiView.Layer.ShadowRadius = ShadowRadius;
            }
            var uiLabel = new UILabel
            {
                Lines = MaxMessageLines,
                Font = UIFont.SystemFontOfSize(FontSize),
                LineBreakMode = UILineBreakMode.WordWrap,
                TextColor = TextColor,
                BackgroundColor = UIColor.Clear,
                Alpha = 1f,
                Text = Content.ToString()
            };

            UpdateFrame(uiView, uiLabel);
            _label = uiLabel;
            return uiView;
        }

        protected CGPoint GetCenterPoint(UIView toast)
        {
            if (CustomPosition.HasValue)
                return CustomPosition.Value;
            var width = Owner.Frame.Size.Width;
            var height = Owner.Frame.Size.Height;

            if (Owner is UIScrollView)
            {
                var uIScrollView = Owner as UIScrollView;
                if (PlatformExtensions.IsOS7)
                    height -= uIScrollView.ScrollIndicatorInsets.Top;
            }

            switch (Position)
            {
                case ToastPosition.Top:
                    nfloat offset = 0f;
                    if (Owner is UIWindow)
                    {
                        if (!UIApplication.SharedApplication.StatusBarHidden)
                            offset = UIApplication.SharedApplication.StatusBarFrame.Height;
                        if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController)
                            offset += 44f;
                    }
                    return new CGPoint(width / 2f, toast.Frame.Size.Height / 2f + VerticalPadding + offset);
                case ToastPosition.Bottom:
                    return new CGPoint(width / 2f, height - toast.Frame.Size.Height / 2f - VerticalPadding);
                case ToastPosition.Center:
                    return new CGPoint(width / 2f, height / 2f);
                default:
                    return CGPoint.Empty;
            }
        }

        protected CGPoint GetCenterPointRotated(UIView view, float rotation)
        {
            var width = Owner.Frame.Size.Width;
            var height = Owner.Frame.Size.Height;

            nfloat offset = 0f;
            if (Owner is UIWindow)
            {
                if (!UIApplication.SharedApplication.StatusBarHidden)
                {
                    if (rotation == 90f || rotation == -90f)
                        offset = UIApplication.SharedApplication.StatusBarFrame.Width;
                    else
                        offset = UIApplication.SharedApplication.StatusBarFrame.Height;
                }
                if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController)
                    offset += 44f;
            }

            switch (Position)
            {
                case ToastPosition.Center:
                    return new CGPoint(width / 2f - VerticalPadding, height / 2f);
                case ToastPosition.Bottom:
                    if (rotation == 90f)
                        return new CGPoint(view.Bounds.Height / 2f + VerticalPadding, height / 2f);
                    if (rotation == -90f)
                        return new CGPoint(width - (view.Bounds.Height / 2f + VerticalPadding), height / 2f);
                    if (rotation == 180f)
                        return new CGPoint(width / 2f, view.Bounds.Height / 2f + VerticalPadding);
                    break;
                case ToastPosition.Top:
                    if (rotation == 90f)
                        return new CGPoint(width - (view.Bounds.Height / 2f + offset + VerticalPadding), height / 2f);
                    if (rotation == -90f)
                        return new CGPoint(view.Bounds.Height / 2f + offset + VerticalPadding, height / 2f);
                    if (rotation == 180f)
                        return new CGPoint(width / 2f, height - view.Bounds.Height / 2f - offset - VerticalPadding);
                    break;
            }
            return GetCenterPoint(view);
        }

        private void UpdateFrame(UIView uiView, UILabel uiLabel)
        {
            uiView.Transform = CGAffineTransform.MakeIdentity();
            if (uiLabel.Superview != null)
                uiLabel.RemoveFromSuperview();

            var size = GetOwnerBoundsSize();
            var constrainedToSize = new CGSize(size.Width * 0.8f, size.Height * 0.8f);
            var cgSize = uiLabel.Text.StringSize(uiLabel.Font, constrainedToSize, uiLabel.LineBreakMode);
            uiLabel.Frame = new CGRect(0f, 0f, cgSize.Width, cgSize.Height);

            var width = uiLabel.Bounds.Size.Width;
            var height = uiLabel.Bounds.Size.Height;
            uiView.Frame = new CGRect(0f, 0f, width + HorizontalPadding * 2f, height + VerticalPadding * 2f);
            uiLabel.Frame = new CGRect(HorizontalPadding, VerticalPadding, width, height);
            uiView.AddSubview(uiLabel);
        }

        private void UpdateWindowOrientation(UIView view)
        {
            if (!view.IsAlive())
                return;
            float num = 0f;
            if (!PlatformExtensions.IsOS8)
            {
                switch (UIApplication.SharedApplication.StatusBarOrientation)
                {
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        num = 180f;
                        break;
                    case UIInterfaceOrientation.LandscapeRight:
                        num = 90f;
                        break;
                    case UIInterfaceOrientation.LandscapeLeft:
                        num = -90f;
                        break;
                }
            }

            if (num == 0f)
            {
                view.Transform = CGAffineTransform.MakeIdentity();
                view.Center = GetCenterPoint(view);
            }
            else
            {
                view.Transform = CGAffineTransform.MakeRotation((float)(num / 180f * Math.PI));
                view.Center = GetCenterPointRotated(view, num);
            }
        }

        private void ShowCompleted()
        {
            if (View.IsAlive())
                UIView.Animate(AnimationDuration, DisplayDuration, UIViewAnimationOptions.CurveEaseIn,
                    () => View.Alpha = 0f, Hide);
        }

        private CGSize GetOwnerBoundsSize()
        {
            if (!(Owner is UIWindow))
                return Owner.Bounds.Size;

            var rotation = UIApplication.SharedApplication.StatusBarOrientation;
            if (!PlatformExtensions.IsOS8 && (rotation == UIInterfaceOrientation.LandscapeRight || rotation == UIInterfaceOrientation.LandscapeLeft))
                return new CGSize(Owner.Bounds.Size.Height, Owner.Bounds.Size.Width);
            return Owner.Bounds.Size;
        }

        #endregion

        #region Implementation of interfaces

        public virtual void OnOrientationChanged()
        {
            if (_label != null)
                UpdateFrame(View, _label);
            UpdateWindowOrientation(View);
        }

        public virtual void Dispose()
        {
            TaskCompletionSource.TrySetResult(null);
            View.RemoveFromSuperview();            
#if XAMARIN_FORMS
            View.Dispose();
#else
            View.ClearBindingsRecursively(true, true);
            View.DisposeEx();
#endif
            PlatformExtensions.RemoveOrientationChangeListener(this);
            ServiceProvider.AttachedValueProvider.Clear(this);
            _label = null;
        }

        #endregion
    }
}
