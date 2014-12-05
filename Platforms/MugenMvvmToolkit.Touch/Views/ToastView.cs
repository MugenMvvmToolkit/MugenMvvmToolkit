#region Copyright
// ****************************************************************************
// <copyright file="ToastView.cs">
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
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Views
{
    public class ToastView : NSObject, IOrientationChangeListener
    {
        #region Fields

        private int _disposed;
        private UILabel _label;

        #endregion

        #region Constructors

        public ToastView(object content, UIView owner, float displayDuration, ToastPosition position,
            TaskCompletionSource<object> tcs)
        {
            Should.NotBeNull(tcs, "tcs");
            Content = content;
            Owner = owner ?? UIApplication.SharedApplication.KeyWindow;
            DisplayDuration = displayDuration / 1000;
            Position = position;
            TaskCompletionSource = tcs;
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

        public float ShadowOffsetX { get; set; }

        public float ShadowOffsetY { get; set; }

        public float ShadowRadius { get; set; }

        public float VerticalPadding { get; set; }

        public float HorizontalPadding { get; set; }

        public PointF? CustomPosition { get; set; }

        public int MaxMessageLines { get; set; }

        public float FontSize { get; set; }

        public float CornerRadius { get; set; }

        public UIColor BackgroundColor { get; set; }

        public UIColor TextColor { get; set; }

        public CGColor ShadowColor { get; set; }

        public UIView Owner { get; protected set; }

        protected object Content { get; set; }

        protected UIView View { get; set; }

        protected float DisplayDuration { get; set; }

        protected ToastPosition Position { get; set; }

        protected TaskCompletionSource<object> TaskCompletionSource { get; set; }

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
                uiView.Layer.ShadowOffset = new SizeF(ShadowOffsetX, ShadowOffsetY);
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

        protected PointF GetCenterPoint(UIView toast)
        {
            if (CustomPosition.HasValue)
                return CustomPosition.Value;
            float width = Owner.Frame.Size.Width;
            float height = Owner.Frame.Size.Height;

            if (Owner is UIScrollView)
            {
                var uIScrollView = Owner as UIScrollView;
                if (PlatformExtensions.IsOS7)
                    height -= uIScrollView.ScrollIndicatorInsets.Top;
            }

            switch (Position)
            {
                case ToastPosition.Top:
                    float offset = 0f;
                    if (Owner is UIWindow)
                    {
                        if (!UIApplication.SharedApplication.StatusBarHidden)
                            offset = UIApplication.SharedApplication.StatusBarFrame.Height;
                        if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController)
                            offset += 44f;
                    }
                    return new PointF(width / 2f, toast.Frame.Size.Height / 2f + VerticalPadding + offset);
                case ToastPosition.Bottom:
                    return new PointF(width / 2f, height - toast.Frame.Size.Height / 2f - VerticalPadding);
                case ToastPosition.Center:
                    return new PointF(width / 2f, height / 2f);
                default:
                    return PointF.Empty;
            }
        }

        protected PointF GetCenterPointRotated(UIView view, float rotation)
        {
            float width = Owner.Frame.Size.Width;
            float height = Owner.Frame.Size.Height;

            float offset = 0f;
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
                    return new PointF(width / 2f - VerticalPadding, height / 2f);
                case ToastPosition.Bottom:
                    if (rotation == 90f)
                        return new PointF(view.Bounds.Height / 2f + VerticalPadding, height / 2f);
                    if (rotation == -90f)
                        return new PointF(width - (view.Bounds.Height / 2f + VerticalPadding), height / 2f);
                    if (rotation == 180f)
                        return new PointF(width / 2f, view.Bounds.Height / 2f + VerticalPadding);
                    break;
                case ToastPosition.Top:
                    if (rotation == 90f)
                        return new PointF(width - (view.Bounds.Height / 2f + offset + VerticalPadding), height / 2f);
                    if (rotation == -90f)
                        return new PointF(view.Bounds.Height / 2f + offset + VerticalPadding, height / 2f);
                    if (rotation == 180f)
                        return new PointF(width / 2f, height - view.Bounds.Height / 2f - offset - VerticalPadding);
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
            var constrainedToSize = new SizeF(size.Width * 0.8f, size.Height * 0.8f);
            SizeF sizeF = uiLabel.StringSize(uiLabel.Text, uiLabel.Font, constrainedToSize, uiLabel.LineBreakMode);
            uiLabel.Frame = new RectangleF(0f, 0f, sizeF.Width, sizeF.Height);

            float width = uiLabel.Bounds.Size.Width;
            float height = uiLabel.Bounds.Size.Height;
            uiView.Frame = new RectangleF(0f, 0f, width + HorizontalPadding * 2f, height + VerticalPadding * 2f);
            uiLabel.Frame = new RectangleF(HorizontalPadding, VerticalPadding, width, height);
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

        private SizeF GetOwnerBoundsSize()
        {
            if (!(Owner is UIWindow))
                return Owner.Bounds.Size;

            var rotation = UIApplication.SharedApplication.StatusBarOrientation;
            if (!PlatformExtensions.IsOS8 && (rotation == UIInterfaceOrientation.LandscapeRight || rotation == UIInterfaceOrientation.LandscapeLeft))
                return new SizeF(Owner.Bounds.Size.Height, Owner.Bounds.Size.Width);
            return Owner.Bounds.Size;
        }

        #endregion

        #region Overrides of NSObject

        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                TaskCompletionSource.TrySetResult(null);
                View.RemoveFromSuperview();
                View.Dispose();
                PlatformExtensions.RemoveOrientationChangeListener(this);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Implementation of IOrientationChangeListener

        public virtual void OnOrientationChanged()
        {
            if (_label != null)
                UpdateFrame(View, _label);
            UpdateWindowOrientation(View);
        }

        #endregion
    }
}